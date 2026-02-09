using SaaS.Application.DTOs;
using SaaS.Domain.Entities;

namespace SaaS.Application.Common.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByCodeAsync(string code, Guid tenantId, CancellationToken cancellationToken = default);
    Task<Product?> GetBySKUAsync(string sku, Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<Product>> GetAllByTenantAsync(Guid tenantId, ProductFilters? filters = null, CancellationToken cancellationToken = default);
    Task<List<Product>> GetActiveByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
