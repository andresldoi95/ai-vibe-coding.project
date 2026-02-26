using SaaS.Domain.Entities;

namespace SaaS.Application.Common.Interfaces;

public interface ISriErrorLogRepository : IRepository<SriErrorLog>
{
    /// <summary>
    /// Gets all error logs for a specific invoice
    /// </summary>
    Task<List<SriErrorLog>> GetByInvoiceIdAsync(Guid invoiceId, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all error logs for a specific credit note
    /// </summary>
    Task<List<SriErrorLog>> GetByCreditNoteIdAsync(Guid creditNoteId, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets error logs for a specific operation type
    /// </summary>
    Task<List<SriErrorLog>> GetByOperationAsync(string operation, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent error logs (last N days)
    /// </summary>
    Task<List<SriErrorLog>> GetRecentErrorsAsync(int days, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets error statistics by operation type
    /// </summary>
    Task<Dictionary<string, int>> GetErrorStatisticsAsync(DateTime fromDate, DateTime toDate, Guid tenantId, CancellationToken cancellationToken = default);
}
