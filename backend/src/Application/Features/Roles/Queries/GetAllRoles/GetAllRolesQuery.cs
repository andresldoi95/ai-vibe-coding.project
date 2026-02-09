using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Roles.Queries.GetAllRoles;

/// <summary>
/// Query to get all roles for the current tenant
/// </summary>
public class GetAllRolesQuery : IRequest<Result<List<RoleWithPermissionsDto>>>
{
}
