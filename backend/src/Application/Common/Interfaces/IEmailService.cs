using SaaS.Application.Common.Models;

namespace SaaS.Application.Common.Interfaces;

public interface IEmailService
{
    Task<Result> SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default);
    Task<Result> SendTemplateEmailAsync(string templateName, string to, Dictionary<string, string> variables, CancellationToken cancellationToken = default);
    Task<Result> SendUserInvitationAsync(string email, string companyName, string inviteToken, Guid tenantId);
    Task<Result> SendPasswordResetAsync(string email, string resetToken);
    Task<Result> SendEmailVerificationAsync(string email, string verificationToken);
    Task<Result> SendWelcomeEmailAsync(string email, string userName, Guid tenantId);
}
