using SaaS.Domain.Entities;

namespace SaaS.Application.Common.Interfaces;

public interface IEmissionPointRepository : IRepository<EmissionPoint>
{
    Task<List<EmissionPoint>> GetByEstablishmentIdAsync(Guid establishmentId, CancellationToken cancellationToken = default);
    Task<EmissionPoint?> GetByCodeAsync(string code, Guid establishmentId, CancellationToken cancellationToken = default);
}
