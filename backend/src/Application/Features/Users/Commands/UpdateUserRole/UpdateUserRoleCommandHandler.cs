using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Users.Commands.UpdateUserRole;

/// <summary>
/// Handler for updating a user's role in the current company
/// </summary>
public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, Result<Unit>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<UpdateUserRoleCommandHandler> _logger;

    public UpdateUserRoleCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<UpdateUserRoleCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<Unit>.Failure("Tenant context is required");
            }

            // Get user-tenant association
            var userTenant = await _unitOfWork.UserTenants.GetByUserAndTenantAsync(
                request.UserId,
                _tenantContext.TenantId.Value,
                cancellationToken);

            if (userTenant == null || !userTenant.IsActive)
            {
                return Result<Unit>.Failure("User not found in this company");
            }

            // Verify new role exists and belongs to current tenant
            var newRole = await _unitOfWork.Roles.GetByIdAsync(request.NewRoleId, cancellationToken);
            if (newRole == null || newRole.TenantId != _tenantContext.TenantId.Value)
            {
                return Result<Unit>.Failure("Invalid role selected");
            }

            // Prevent demoting the last Owner
            if (userTenant.Role!.Name == "Owner" && newRole.Name != "Owner")
            {
                var ownerRole = await _unitOfWork.Roles.GetAllAsync(cancellationToken);
                var ownerRoleId = ownerRole.FirstOrDefault(r => r.TenantId == _tenantContext.TenantId.Value && r.Name == "Owner")?.Id;

                if (ownerRoleId.HasValue)
                {
                    var allUserTenants = await _unitOfWork.UserTenants.GetByTenantIdWithDetailsAsync(
                        _tenantContext.TenantId.Value,
                        cancellationToken);

                    var ownerCount = allUserTenants.Count(ut => ut.RoleId == ownerRoleId.Value && ut.IsActive);

                    if (ownerCount <= 1)
                    {
                        return Result<Unit>.Failure("Cannot change the role of the last owner. Please assign another owner first.");
                    }
                }
            }

            // Update role
            userTenant.RoleId = request.NewRoleId;
            await _unitOfWork.UserTenants.UpdateAsync(userTenant, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated role for user {UserId} in tenant {TenantId} to role {NewRoleId}",
                request.UserId, _tenantContext.TenantId.Value, request.NewRoleId);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role for user {UserId}", request.UserId);
            return Result<Unit>.Failure("Failed to update user role. Please try again.");
        }
    }
}
