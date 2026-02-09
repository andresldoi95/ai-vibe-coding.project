using MediatR;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Roles.Queries.GetAllRoles;

public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, Result<List<RoleWithPermissionsDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetAllRolesQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result<List<RoleWithPermissionsDto>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        if (tenantId == null || tenantId == Guid.Empty)
        {
            return Result<List<RoleWithPermissionsDto>>.Failure("Tenant context not set");
        }

        var roles = await _unitOfWork.Roles.GetAllWithPermissionsByTenantAsync(tenantId.Value, cancellationToken);

        var roleDtos = new List<RoleWithPermissionsDto>();
        foreach (var r in roles)
        {
            var userCount = await _unitOfWork.Roles.GetUserCountAsync(r.Id, cancellationToken);
            roleDtos.Add(new RoleWithPermissionsDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Priority = r.Priority,
                IsSystemRole = r.IsSystemRole,
                IsActive = r.IsActive,
                UserCount = userCount,
                Permissions = r.RolePermissions
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
            });
        }

        return Result<List<RoleWithPermissionsDto>>.Success(roleDtos);
    }
}
