using Microsoft.EntityFrameworkCore;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace SaaS.Infrastructure.Persistence.Repositories;

public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Invoice?> GetByNumberAsync(string invoiceNumber, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Customer)
            .Include(i => i.Warehouse)
            .Include(i => i.Items)
            .FirstOrDefaultAsync(
                i => i.InvoiceNumber == invoiceNumber && i.TenantId == tenantId && !i.IsDeleted,
                cancellationToken);
    }

    public async Task<List<Invoice>> GetByCustomerAsync(Guid customerId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Customer)
            .Where(i => i.CustomerId == customerId && i.TenantId == tenantId && !i.IsDeleted)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Invoice>> GetByStatusAsync(InvoiceStatus status, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Customer)
            .Where(i => i.Status == status && i.TenantId == tenantId && !i.IsDeleted)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Invoice?> GetWithItemsAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Customer)
            .Include(i => i.Warehouse)
            .Include(i => i.Items)
                .ThenInclude(ii => ii.Product)
            .Include(i => i.Items)
                .ThenInclude(ii => ii.TaxRateEntity)
            .FirstOrDefaultAsync(
                i => i.Id == id && i.TenantId == tenantId && !i.IsDeleted,
                cancellationToken);
    }
}
