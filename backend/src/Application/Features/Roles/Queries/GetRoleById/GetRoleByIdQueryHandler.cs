using MediatR;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Roles.Queries.GetRoleById;

public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, Result<RoleWithPermissionsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetRoleByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result<RoleWithPermissionsDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
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

        var roleDto = new RoleWithPermissionsDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            Priority = role.Priority,
            IsSystemRole = role.IsSystemRole,
            IsActive = role.IsActive,
            UserCount = 0, // TODO: Query user count separately if needed
            Permissions = role.RolePermissions
                .Select(rp => new PermissionDto
                {
                    Id = rp.Permission.Id,
                    Name = rp.Permission.Name,
                    Description = rp.Permission.Description,
                    Resource = rp.Permission.Resource,
                    Action = rp.Permission.Action
                })
                .OrderBy(p => p.Resource)
                .ThenBy(p => p.Action)
                .ToList()
        };

        return Result<RoleWithPermissionsDto>.Success(roleDto);
    }
}
