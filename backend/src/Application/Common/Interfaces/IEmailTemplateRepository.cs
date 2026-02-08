using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace SaaS.Application.Common.Interfaces;

public interface IEmailTemplateRepository : IRepository<EmailTemplate>
{
    Task<EmailTemplate?> GetByNameAsync(string name, Guid tenantId);
    Task<List<EmailTemplate>> GetByTypeAsync(EmailType type, Guid tenantId);
    Task<bool> ExistsAsync(string name, Guid tenantId);
}
