using Microsoft.EntityFrameworkCore;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Repositories;

public class TenantRepository : Repository<Tenant>, ITenantRepository
{
    public TenantRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(t => t.Slug == slug, cancellationToken);
    }

    public async Task<List<Tenant>> GetUserTenantsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserTenants
            .Where(ut => ut.UserId == userId && ut.IsActive)
            .Select(ut => ut.Tenant)
            .ToListAsync(cancellationToken);
    }
}
