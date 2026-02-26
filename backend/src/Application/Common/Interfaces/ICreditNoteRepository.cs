using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace SaaS.Application.Common.Interfaces;

public interface ICreditNoteRepository : IRepository<CreditNote>
{
    Task<CreditNote?> GetByNumberAsync(string creditNoteNumber, Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<CreditNote>> GetByCustomerAsync(Guid customerId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<CreditNote>> GetByStatusAsync(InvoiceStatus status, Guid tenantId, CancellationToken cancellationToken = default);
    Task<CreditNote?> GetWithItemsAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);

    // Background job methods (cross-tenant)
    Task<List<CreditNote>> GetAllByStatusAsync(InvoiceStatus status, CancellationToken cancellationToken = default);
}
