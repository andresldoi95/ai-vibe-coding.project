using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Roles.Commands.CreateRole;

public class CreateRoleCommand : IRequest<Result<RoleWithPermissionsDto>>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Priority { get; set; } = 10;
    public List<Guid> PermissionIds { get; set; } = new();
}
