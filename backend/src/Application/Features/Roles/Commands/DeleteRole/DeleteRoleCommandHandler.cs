using MediatR;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Roles.Commands.DeleteRole;

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public DeleteRoleCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result<bool>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        if (tenantId == null || tenantId == Guid.Empty)
        {
            return Result<bool>.Failure("Tenant context not set");
        }

        var role = await _unitOfWork.Roles.GetByIdWithPermissionsAsync(request.Id, cancellationToken);

        if (role == null || role.TenantId != tenantId)
        {
            return Result<bool>.Failure("Role not found");
        }

        // Cannot delete system roles
        if (role.IsSystemRole)
        {
            return Result<bool>.Failure("System roles cannot be deleted");
        }

        // Cannot delete role if users are assigned to it
        var userCount = await _unitOfWork.Roles.GetUserCountAsync(request.Id, cancellationToken);
        if (userCount > 0)
        {
            return Result<bool>.Failure($"Cannot delete role. {userCount} user(s) are assigned to this role");
        }

        // Soft delete
        role.IsDeleted = true;
        role.DeletedAt = DateTime.UtcNow;
        role.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Roles.UpdateAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
