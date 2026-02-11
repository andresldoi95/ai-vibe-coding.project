using Microsoft.EntityFrameworkCore;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Repositories;

public class InvoiceConfigurationRepository : Repository<InvoiceConfiguration>, IInvoiceConfigurationRepository
{
    public InvoiceConfigurationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<InvoiceConfiguration?> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                ic => ic.TenantId == tenantId && !ic.IsDeleted,
                cancellationToken);
    }
}
