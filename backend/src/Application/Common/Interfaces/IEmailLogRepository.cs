using SaaS.Domain.Entities;

namespace SaaS.Application.Common.Interfaces;

public interface IEmailLogRepository : IRepository<EmailLog>
{
    Task<List<EmailLog>> GetByUserIdAsync(Guid userId, Guid tenantId);
    Task<List<EmailLog>> GetFailedEmailsAsync(Guid tenantId);
    Task<List<EmailLog>> GetPendingEmailsAsync(Guid tenantId);
}
