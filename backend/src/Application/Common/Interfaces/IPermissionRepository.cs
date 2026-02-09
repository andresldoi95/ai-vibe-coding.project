using SaaS.Domain.Entities;

namespace SaaS.Application.Common.Interfaces;

public interface IPermissionRepository
{
    Task<List<Permission>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Permission>> GetByIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default);
}
