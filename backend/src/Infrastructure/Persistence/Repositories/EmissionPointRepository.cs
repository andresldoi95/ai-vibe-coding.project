using Microsoft.EntityFrameworkCore;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Repositories;

public class EmissionPointRepository : Repository<EmissionPoint>, IEmissionPointRepository
{
    public EmissionPointRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<EmissionPoint>> GetByEstablishmentIdAsync(Guid establishmentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.EstablishmentId == establishmentId && !e.IsDeleted)
            .OrderBy(e => e.EmissionPointCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmissionPoint?> GetByCodeAsync(string code, Guid establishmentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                e => e.EmissionPointCode == code && e.EstablishmentId == establishmentId && !e.IsDeleted,
                cancellationToken);
    }
}
