using MediatR;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Entities;

namespace SaaS.Application.Features.Roles.Commands.UpdateRole;

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Result<RoleWithPermissionsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateRoleCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result<RoleWithPermissionsDto>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        if (tenantId == null || tenantId == Guid.Empty)
        {
            return Result<RoleWithPermissionsDto>.Failure("Tenant context not set");
        }

        var role = await _unitOfWork.Roles.GetByIdWithPermissionsAsync(request.Id, cancellationToken);

        if (role == null || role.TenantId != tenantId)
        {
            return Result<RoleWithPermissionsDto>.Failure("Role not found");
        }

        // Prevent editing system role core properties (allow permission updates)
        if (role.IsSystemRole)
        {
            // Only allow permission updates for system roles
            if (role.Name != request.Name || role.Priority != request.Priority)
            {
                return Result<RoleWithPermissionsDto>.Failure("System roles cannot have their name or priority changed");
            }
        }
        else
        {
            // For custom roles, check name uniqueness
            var duplicateName = await _unitOfWork.Roles.ExistsByNameAsync(request.Name, tenantId.Value, request.Id, cancellationToken);

            if (duplicateName)
            {
                return Result<RoleWithPermissionsDto>.Failure($"Role with name '{request.Name}' already exists");
            }

            role.Name = request.Name;
            role.Priority = request.Priority;
        }

        role.Description = request.Description;
        role.UpdatedAt = DateTime.UtcNow;

        // Update permissions - clear and re-add
        role.RolePermissions.Clear();
        role.RolePermissions = request.PermissionIds.Select(pid => new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = role.Id,
            PermissionId = pid
        }).ToList();

        await _unitOfWork.Roles.UpdateAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Load permissions for response
        var permissions = await _unitOfWork.Permissions.GetByIdsAsync(request.PermissionIds, cancellationToken);

        var roleDto = new RoleWithPermissionsDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            Priority = role.Priority,
            IsSystemRole = role.IsSystemRole,
            IsActive = role.IsActive,
            UserCount = 0, // TODO: Query user count separately if needed
            Permissions = permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Resource = p.Resource,
                Action = p.Action
            }).ToList()
        };

        return Result<RoleWithPermissionsDto>.Success(roleDto);
    }
}
