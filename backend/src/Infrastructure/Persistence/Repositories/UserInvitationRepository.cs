using Microsoft.EntityFrameworkCore;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Repositories;

public class UserInvitationRepository : Repository<UserInvitation>, IUserInvitationRepository
{
    public UserInvitationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<UserInvitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.UserInvitations
            .Include(ui => ui.Tenant)
            .Include(ui => ui.Role)
            .Include(ui => ui.InvitedByUser)
            .FirstOrDefaultAsync(ui => ui.InvitationToken == token, cancellationToken);
    }

    public async Task<UserInvitation?> GetActiveByEmailAndTenantAsync(string email, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.UserInvitations
            .FirstOrDefaultAsync(
                ui => ui.Email == email &&
                      ui.TenantId == tenantId &&
                      ui.IsActive &&
                      ui.ExpiresAt > DateTime.UtcNow &&
                      ui.AcceptedAt == null,
                cancellationToken);
    }
}
