using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Permissions.Queries.GetAllPermissions;

/// <summary>
/// Query to get all available permissions
/// </summary>
public class GetAllPermissionsQuery : IRequest<Result<List<PermissionDto>>>
{
}
