using SaaS.Domain.Entities;

namespace SaaS.Application.Common.Interfaces;

public interface IUserTenantRepository : IRepository<UserTenant>
{
    Task<UserTenant?> GetWithRoleAndPermissionsAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<UserTenant>> GetByTenantIdWithDetailsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<UserTenant?> GetByUserAndTenantAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
}
