using SaaS.Domain.Entities;

namespace SaaS.Application.Common.Interfaces;

public interface IWarehouseRepository : IRepository<Warehouse>
{
    Task<Warehouse?> GetByCodeAsync(string code, Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<Warehouse>> GetAllByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<Warehouse>> GetActiveByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
