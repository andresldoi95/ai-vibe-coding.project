using SaaS.Domain.Entities;

namespace SaaS.Application.Common.Interfaces;

public interface IRoleRepository : IRepository<Role>
{
    Task<List<Role>> GetAllWithPermissionsByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Role?> GetByIdWithPermissionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Role?> GetByNameAsync(string name, Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, Guid tenantId, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> GetUserCountAsync(Guid roleId, CancellationToken cancellationToken = default);
}
