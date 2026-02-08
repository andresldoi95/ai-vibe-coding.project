using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace SaaS.Infrastructure.Persistence.Repositories;

public class EmailTemplateRepository : Repository<EmailTemplate>, IEmailTemplateRepository
{
    public EmailTemplateRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<EmailTemplate?> GetByNameAsync(string name, Guid tenantId)
    {
        return await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.Name == name && t.TenantId == tenantId);
    }

    public async Task<List<EmailTemplate>> GetByTypeAsync(EmailType type, Guid tenantId)
    {
        return await _context.EmailTemplates
            .Where(t => t.Type == type && t.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string name, Guid tenantId)
    {
        return await _context.EmailTemplates
            .AnyAsync(t => t.Name == name && t.TenantId == tenantId);
    }
}
