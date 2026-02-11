using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace SaaS.Application.Common.Interfaces;

public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<Invoice?> GetByNumberAsync(string invoiceNumber, Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<Invoice>> GetByCustomerAsync(Guid customerId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<Invoice>> GetByStatusAsync(InvoiceStatus status, Guid tenantId, CancellationToken cancellationToken = default);
    Task<Invoice?> GetWithItemsAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
}
