namespace SaaS.Application.Common.Interfaces;

public interface ISriConfigurationRepository : IRepository<Domain.Entities.SriConfiguration>
{
    Task<Domain.Entities.SriConfiguration?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
