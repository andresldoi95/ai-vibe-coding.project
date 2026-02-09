using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Roles.Queries.GetRoleById;

public class GetRoleByIdQuery : IRequest<Result<RoleWithPermissionsDto>>
{
    public Guid Id { get; set; }
}
