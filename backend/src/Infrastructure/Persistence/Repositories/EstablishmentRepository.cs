using Microsoft.EntityFrameworkCore;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Repositories;

public class EstablishmentRepository : Repository<Establishment>, IEstablishmentRepository
{
    public EstablishmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<Establishment>> GetAllByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.TenantId == tenantId && !e.IsDeleted)
            .OrderBy(e => e.EstablishmentCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<Establishment?> GetByCodeAsync(string code, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                e => e.EstablishmentCode == code && e.TenantId == tenantId && !e.IsDeleted,
                cancellationToken);
    }
}
