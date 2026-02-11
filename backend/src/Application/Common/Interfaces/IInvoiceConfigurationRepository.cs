using SaaS.Domain.Entities;

namespace SaaS.Application.Common.Interfaces;

public interface IInvoiceConfigurationRepository : IRepository<InvoiceConfiguration>
{
    Task<InvoiceConfiguration?> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
