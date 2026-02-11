using Microsoft.EntityFrameworkCore;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Repositories;

public class TaxRateRepository : Repository<TaxRate>, ITaxRateRepository
{
    public TaxRateRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<TaxRate?> GetByCodeAsync(string code, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                tr => tr.Code == code && tr.TenantId == tenantId && !tr.IsDeleted,
                cancellationToken);
    }

    public async Task<List<TaxRate>> GetActiveByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(tr => tr.TenantId == tenantId && tr.IsActive && !tr.IsDeleted)
            .OrderBy(tr => tr.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<TaxRate?> GetDefaultRateAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                tr => tr.TenantId == tenantId && tr.IsDefault && tr.IsActive && !tr.IsDeleted,
                cancellationToken);
    }
}
