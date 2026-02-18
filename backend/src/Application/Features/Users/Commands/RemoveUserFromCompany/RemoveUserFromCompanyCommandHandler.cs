using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Users.Commands.RemoveUserFromCompany;

/// <summary>
/// Handler for removing a user from the current company
/// </summary>
public class RemoveUserFromCompanyCommandHandler : IRequestHandler<RemoveUserFromCompanyCommand, Result<Unit>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<RemoveUserFromCompanyCommandHandler> _logger;

    public RemoveUserFromCompanyCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<RemoveUserFromCompanyCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(RemoveUserFromCompanyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<Unit>.Failure("Tenant context is required");
            }

            if (!_tenantContext.UserId.HasValue)
            {
                return Result<Unit>.Failure("User context is required");
            }

            var currentUserId = _tenantContext.UserId.Value;

            // Prevent self-removal
            if (request.UserId == currentUserId)
            {
                return Result<Unit>.Failure("You cannot remove yourself from the company");
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

            // Prevent removing the last Owner
            if (userTenant.Role!.Name == "Owner")
            {
                var allUserTenants = await _unitOfWork.UserTenants.GetByTenantIdWithDetailsAsync(
                    _tenantContext.TenantId.Value,
                    cancellationToken);

                var ownerCount = allUserTenants.Count(ut => ut.Role!.Name == "Owner" && ut.IsActive);

                if (ownerCount <= 1)
                {
                    return Result<Unit>.Failure("Cannot remove the last owner. Please assign another owner first.");
                }
            }

            // Soft delete: set IsActive to false
            userTenant.IsActive = false;
            await _unitOfWork.UserTenants.UpdateAsync(userTenant, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Removed user {UserId} from tenant {TenantId}", request.UserId, _tenantContext.TenantId.Value);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {UserId} from company", request.UserId);
            return Result<Unit>.Failure("Failed to remove user. Please try again.");
        }
    }
}
