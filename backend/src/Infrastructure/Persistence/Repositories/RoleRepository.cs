using Microsoft.EntityFrameworkCore;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Repositories;

public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<Role>> GetAllWithPermissionsByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Where(r => r.TenantId == tenantId && !r.IsDeleted)
            .OrderByDescending(r => r.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<Role?> GetByIdWithPermissionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
    }

    public async Task<Role?> GetByNameAsync(string name, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.Name == name && r.TenantId == tenantId && !r.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid tenantId, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(r => r.Name == name && r.TenantId == tenantId && !r.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<int> GetUserCountAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _context.UserTenants
            .CountAsync(ut => ut.RoleId.HasValue && ut.RoleId.Value == roleId && ut.IsActive, cancellationToken);
    }
}
