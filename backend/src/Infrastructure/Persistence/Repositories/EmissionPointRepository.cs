using Microsoft.EntityFrameworkCore;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Repositories;

public class EmissionPointRepository : Repository<EmissionPoint>, IEmissionPointRepository
{
    public EmissionPointRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<EmissionPoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Establishment)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
    }

    public override async Task<IReadOnlyList<EmissionPoint>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Establishment)
            .Where(e => !e.IsDeleted)
            .OrderBy(e => e.Establishment.EstablishmentCode)
            .ThenBy(e => e.EmissionPointCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EmissionPoint>> GetByEstablishmentIdAsync(Guid establishmentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Establishment)
            .Where(e => e.EstablishmentId == establishmentId && !e.IsDeleted)
            .OrderBy(e => e.EmissionPointCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmissionPoint?> GetByCodeAsync(string code, Guid establishmentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Establishment)
            .FirstOrDefaultAsync(
                e => e.EmissionPointCode == code && e.EstablishmentId == establishmentId && !e.IsDeleted,
                cancellationToken);
    }
}
