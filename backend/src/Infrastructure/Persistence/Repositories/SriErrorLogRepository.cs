using Microsoft.EntityFrameworkCore;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Repositories;

public class SriErrorLogRepository : Repository<SriErrorLog>, ISriErrorLogRepository
{
    public SriErrorLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<SriErrorLog>> GetByInvoiceIdAsync(Guid invoiceId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.InvoiceId == invoiceId && e.TenantId == tenantId && !e.IsDeleted)
            .OrderByDescending(e => e.OccurredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SriErrorLog>> GetByOperationAsync(string operation, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Invoice)
            .Where(e => e.Operation == operation && e.TenantId == tenantId && !e.IsDeleted)
            .OrderByDescending(e => e.OccurredAt)
            .Take(100) // Limit to last 100 errors
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SriErrorLog>> GetRecentErrorsAsync(int days, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);

        return await _dbSet
            .Include(e => e.Invoice)
            .Where(e => e.OccurredAt >= cutoffDate && e.TenantId == tenantId && !e.IsDeleted)
            .OrderByDescending(e => e.OccurredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<string, int>> GetErrorStatisticsAsync(DateTime fromDate, DateTime toDate, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.OccurredAt >= fromDate && e.OccurredAt <= toDate && e.TenantId == tenantId && !e.IsDeleted)
            .GroupBy(e => e.Operation)
            .Select(g => new { Operation = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Operation, x => x.Count, cancellationToken);
    }
}
