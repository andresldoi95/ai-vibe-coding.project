using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Tenants.Queries.GetUserTenants;

/// <summary>
/// Query to get all tenants for a user
/// </summary>
public record GetUserTenantsQuery : IRequest<Result<List<TenantDto>>>
{
    public Guid UserId { get; init; }
}
