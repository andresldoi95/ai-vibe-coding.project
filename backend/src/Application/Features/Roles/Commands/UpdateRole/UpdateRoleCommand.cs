using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Roles.Commands.UpdateRole;

public class UpdateRoleCommand : IRequest<Result<RoleWithPermissionsDto>>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Priority { get; set; }
    public List<Guid> PermissionIds { get; set; } = new();
}
