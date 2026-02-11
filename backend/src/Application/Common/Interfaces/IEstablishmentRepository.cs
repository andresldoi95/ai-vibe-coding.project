using SaaS.Domain.Entities;

namespace SaaS.Application.Common.Interfaces;

public interface IEstablishmentRepository : IRepository<Establishment>
{
    Task<Establishment?> GetByCodeAsync(string code, Guid tenantId, CancellationToken cancellationToken = default);
}
