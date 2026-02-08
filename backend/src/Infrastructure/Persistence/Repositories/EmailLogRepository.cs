using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace SaaS.Infrastructure.Persistence.Repositories;

public class EmailLogRepository : Repository<EmailLog>, IEmailLogRepository
{
    public EmailLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<EmailLog>> GetByUserIdAsync(Guid userId, Guid tenantId)
    {
        return await _context.EmailLogs
            .Where(e => e.UserId == userId && e.TenantId == tenantId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<EmailLog>> GetFailedEmailsAsync(Guid tenantId)
    {
        return await _context.EmailLogs
            .Where(e => e.Status == EmailStatus.Failed && e.TenantId == tenantId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<EmailLog>> GetPendingEmailsAsync(Guid tenantId)
    {
        return await _context.EmailLogs
            .Where(e => e.Status == EmailStatus.Pending && e.TenantId == tenantId)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();
    }
}
