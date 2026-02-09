using MediatR;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Entities;

namespace SaaS.Application.Features.Roles.Commands.CreateRole;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<RoleWithPermissionsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateRoleCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result<RoleWithPermissionsDto>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        if (tenantId == null || tenantId == Guid.Empty)
        {
            return Result<RoleWithPermissionsDto>.Failure("Tenant context not set");
        }

        // Check if role name already exists for this tenant
        var exists = await _unitOfWork.Roles.ExistsByNameAsync(request.Name, tenantId.Value, null, cancellationToken);

        if (exists)
        {
            return Result<RoleWithPermissionsDto>.Failure($"Role with name '{request.Name}' already exists");
        }

        // Create new role
        var role = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            Name = request.Name,
            Description = request.Description,
            Priority = request.Priority,
            IsSystemRole = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RolePermissions = request.PermissionIds.Select(pid => new RolePermission
            {
                Id = Guid.NewGuid(),
                PermissionId = pid
            }).ToList()
        };

        await _unitOfWork.Roles.AddAsync(role, cancellationToken);
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
            UserCount = 0,
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
