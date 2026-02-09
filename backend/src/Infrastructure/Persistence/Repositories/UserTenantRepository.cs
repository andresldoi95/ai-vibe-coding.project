using Microsoft.EntityFrameworkCore;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Repositories;

public class UserTenantRepository : Repository<UserTenant>, IUserTenantRepository
{
    public UserTenantRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<UserTenant?> GetWithRoleAndPermissionsAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.UserTenants
            .Include(ut => ut.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(
                ut => ut.UserId == userId && ut.TenantId == tenantId && ut.IsActive,
                cancellationToken);
    }
}
