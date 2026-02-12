using Microsoft.EntityFrameworkCore;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Repositories;

public class WarehouseRepository : Repository<Warehouse>, IWarehouseRepository
{
    public WarehouseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(w => w.Country)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<Warehouse?> GetByCodeAsync(string code, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(w => w.Country)
            .FirstOrDefaultAsync(
                w => w.Code == code && w.TenantId == tenantId && !w.IsDeleted,
                cancellationToken);
    }

    public async Task<List<Warehouse>> GetAllByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(w => w.Country)
            .Where(w => w.TenantId == tenantId && !w.IsDeleted)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Warehouse>> GetActiveByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(w => w.Country)
            .Where(w => w.TenantId == tenantId && w.IsActive && !w.IsDeleted)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }
}
