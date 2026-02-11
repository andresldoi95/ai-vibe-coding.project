using Microsoft.EntityFrameworkCore;
using SaaS.Application.Common.Interfaces;

namespace SaaS.Infrastructure.Persistence.Repositories;

public class SriConfigurationRepository : Repository<Domain.Entities.SriConfiguration>, ISriConfigurationRepository
{
    public SriConfigurationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Domain.Entities.SriConfiguration?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                s => s.TenantId == tenantId && !s.IsDeleted,
                cancellationToken);
    }
}
