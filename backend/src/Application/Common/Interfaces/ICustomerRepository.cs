using SaaS.Application.DTOs;
using SaaS.Domain.Entities;

namespace SaaS.Application.Common.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email, Guid tenantId, CancellationToken cancellationToken = default);
    Task<Customer?> GetByTaxIdAsync(string taxId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<Customer>> GetAllByTenantAsync(Guid tenantId, CustomerFilters? filters = null, CancellationToken cancellationToken = default);
    Task<List<Customer>> GetActiveByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
