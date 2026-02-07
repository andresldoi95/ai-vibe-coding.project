using SaaS.Domain.Entities;

namespace SaaS.Application.Common.Interfaces;

public interface ITenantRepository : IRepository<Tenant>
{
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<Tenant>> GetUserTenantsAsync(Guid userId, CancellationToken cancellationToken = default);
}
