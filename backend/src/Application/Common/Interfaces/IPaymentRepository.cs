using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace SaaS.Application.Common.Interfaces;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<List<Payment>> GetAllByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<Payment>> GetByInvoiceIdAsync(Guid invoiceId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<Payment>> GetByStatusAsync(PaymentStatus status, Guid tenantId, CancellationToken cancellationToken = default);
}
