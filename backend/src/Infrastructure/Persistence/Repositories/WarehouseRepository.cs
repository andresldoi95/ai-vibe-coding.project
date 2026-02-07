using Microsoft.EntityFrameworkCore;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Repositories;

public class WarehouseRepository : Repository<Warehouse>, IWarehouseRepository
{
    public WarehouseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Warehouse?> GetByCodeAsync(string code, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                w => w.Code == code && w.TenantId == tenantId && !w.IsDeleted,
                cancellationToken);
    }

    public async Task<List<Warehouse>> GetAllByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(w => w.TenantId == tenantId && !w.IsDeleted)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Warehouse>> GetActiveByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(w => w.TenantId == tenantId && w.IsActive && !w.IsDeleted)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }
}
