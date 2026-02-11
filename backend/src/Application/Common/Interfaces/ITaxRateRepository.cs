using SaaS.Domain.Entities;

namespace SaaS.Application.Common.Interfaces;

public interface ITaxRateRepository : IRepository<TaxRate>
{
    Task<TaxRate?> GetByCodeAsync(string code, Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<TaxRate>> GetActiveByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<TaxRate?> GetDefaultRateAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
