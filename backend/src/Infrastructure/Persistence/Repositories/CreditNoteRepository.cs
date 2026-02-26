using Microsoft.EntityFrameworkCore;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace SaaS.Infrastructure.Persistence.Repositories;

public class CreditNoteRepository : Repository<CreditNote>, ICreditNoteRepository
{
    public CreditNoteRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<CreditNote?> GetByNumberAsync(string creditNoteNumber, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(cn => cn.Customer)
            .Include(cn => cn.Items)
            .FirstOrDefaultAsync(
                cn => cn.CreditNoteNumber == creditNoteNumber && cn.TenantId == tenantId && !cn.IsDeleted,
                cancellationToken);
    }

    public async Task<List<CreditNote>> GetByCustomerAsync(Guid customerId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(cn => cn.Customer)
            .Where(cn => cn.CustomerId == customerId && cn.TenantId == tenantId && !cn.IsDeleted)
            .OrderByDescending(cn => cn.IssueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CreditNote>> GetByStatusAsync(InvoiceStatus status, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(cn => cn.Customer)
            .Where(cn => cn.Status == status && cn.TenantId == tenantId && !cn.IsDeleted)
            .OrderByDescending(cn => cn.IssueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<CreditNote?> GetWithItemsAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(cn => cn.Customer)
            .Include(cn => cn.EmissionPoint)
                .ThenInclude(ep => ep!.Establishment)
            .Include(cn => cn.Items)
                .ThenInclude(ii => ii.Product)
            .Include(cn => cn.Items)
                .ThenInclude(ii => ii.TaxRateEntity)
            .FirstOrDefaultAsync(
                cn => cn.Id == id && cn.TenantId == tenantId && !cn.IsDeleted,
                cancellationToken);
    }

    public async Task<List<CreditNote>> GetAllByStatusAsync(InvoiceStatus status, CancellationToken cancellationToken = default)
    {
        // Cross-tenant query for background jobs - no tenant filter
        return await _dbSet
            .Include(cn => cn.Customer)
            .Where(cn => cn.Status == status && !cn.IsDeleted)
            .OrderByDescending(cn => cn.IssueDate)
            .ToListAsync(cancellationToken);
    }
}
