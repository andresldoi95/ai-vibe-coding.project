using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace SaaS.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IEmailTemplateRepository _templateRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> settings,
        IEmailLogRepository emailLogRepository,
        IEmailTemplateRepository templateRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _emailLogRepository = emailLogRepository;
        _templateRepository = templateRepository;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var tenantId = message.TenantId.HasValue && message.TenantId.Value != Guid.Empty
            ? message.TenantId.Value
            : (_tenantContext.TenantId ?? Guid.Empty);

        var emailLog = new EmailLog
        {
            TenantId = tenantId,
            UserId = message.UserId,
            To = message.To,
            Cc = message.Cc,
            Bcc = message.Bcc,
            Subject = message.Subject,
            Body = message.BodyHtml,
            Type = message.Type,
            Status = EmailStatus.Pending
        };

        try
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            mimeMessage.To.Add(MailboxAddress.Parse(message.To));

            if (!string.IsNullOrEmpty(message.Cc))
                mimeMessage.Cc.Add(MailboxAddress.Parse(message.Cc));

            if (!string.IsNullOrEmpty(message.Bcc))
                mimeMessage.Bcc.Add(MailboxAddress.Parse(message.Bcc));

            mimeMessage.Subject = message.Subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = message.BodyHtml,
                TextBody = message.BodyText ?? StripHtml(message.BodyHtml)
            };

            mimeMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            client.Timeout = _settings.Timeout;

            await client.ConnectAsync(
                _settings.SmtpHost,
                _settings.SmtpPort,
                _settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                cancellationToken);

            if (!string.IsNullOrEmpty(_settings.SmtpUsername))
            {
                await client.AuthenticateAsync(_settings.SmtpUsername, _settings.SmtpPassword, cancellationToken);
            }

            await client.SendAsync(mimeMessage, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            emailLog.Status = EmailStatus.Sent;
            emailLog.SentAt = DateTime.UtcNow;

            _logger.LogInformation("Email sent successfully to {To}", message.To);

            return Result.Success();
        }
        catch (Exception ex)
        {
            emailLog.Status = EmailStatus.Failed;
            emailLog.ErrorMessage = ex.Message;

            _logger.LogError(ex, "Failed to send email to {To}", message.To);

            return Result.Failure($"Failed to send email: {ex.Message}");
        }
        finally
        {
            await _emailLogRepository.AddAsync(emailLog);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<Result> SendTemplateEmailAsync(
        string templateName,
        string to,
        Dictionary<string, string> variables,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId ?? Guid.Empty;
        var template = await _templateRepository.GetByNameAsync(templateName, tenantId);

        if (template == null)
            return Result.Failure($"Email template '{templateName}' not found");

        var subject = ReplaceVariables(template.Subject, variables);
        var bodyHtml = ReplaceVariables(template.BodyHtml, variables);
        var bodyText = template.BodyText != null ? ReplaceVariables(template.BodyText, variables) : null;

        var message = new EmailMessage
        {
            To = to,
            Subject = subject,
            BodyHtml = bodyHtml,
            BodyText = bodyText,
            Type = template.Type,
            TenantId = tenantId == Guid.Empty ? null : tenantId
        };

        return await SendEmailAsync(message, cancellationToken);
    }

    public async Task<Result> SendUserInvitationAsync(string email, string companyName, string inviteToken, Guid tenantId)
    {
        var inviteUrl = $"{_settings.FrontendUrl}/accept-invitation?token={inviteToken}";

        var variables = new Dictionary<string, string>
        {
            { "{inviteeName}", email.Split('@')[0] },
            { "{inviterName}", "Admin" },
            { "{companyName}", companyName },
            { "{invitationLink}", inviteUrl },
            { "{role}", "Team Member" },
            { "{frontendUrl}", _settings.FrontendUrl }
        };

        var message = new EmailMessage
        {
            To = email,
            Subject = $"You're invited to join {companyName}",
            BodyHtml = await GetTemplateContentAsync("UserInvitation", variables),
            Type = EmailType.UserInvitation,
            TenantId = tenantId
        };

        return await SendEmailAsync(message);
    }

    public async Task<Result> SendPasswordResetAsync(string email, string resetToken)
    {
        var resetUrl = $"{_settings.FrontendUrl}/reset-password?token={resetToken}";

        var variables = new Dictionary<string, string>
        {
            { "{userName}", email.Split('@')[0] },
            { "{resetLink}", resetUrl },
            { "{expiryTime}", "1 hour" },
            { "{frontendUrl}", _settings.FrontendUrl }
        };

        var message = new EmailMessage
        {
            To = email,
            Subject = "Reset your password",
            BodyHtml = await GetTemplateContentAsync("PasswordReset", variables),
            Type = EmailType.PasswordReset
        };

        return await SendEmailAsync(message);
    }

    public async Task<Result> SendEmailVerificationAsync(string email, string verificationToken)
    {
        var verifyUrl = $"https://yoursaas.com/verify-email?token={verificationToken}";

        var variables = new Dictionary<string, string>
        {
            { "{VerifyUrl}", verifyUrl },
            { "{Email}", email }
        };

        var message = new EmailMessage
        {
            To = email,
            Subject = "Verify your email address",
            BodyHtml = await GetTemplateContentAsync("EmailVerification", variables),
            Type = EmailType.EmailVerification
        };

        return await SendEmailAsync(message);
    }

    public async Task<Result> SendWelcomeEmailAsync(string email, string userName, Guid tenantId)
    {
        var variables = new Dictionary<string, string>
        {
            { "{userName}", userName },
            { "{companyName}", "SaaS Platform" },
            { "{loginLink}", $"{_settings.FrontendUrl}/login" },
            { "{frontendUrl}", _settings.FrontendUrl }
        };

        var message = new EmailMessage
        {
            To = email,
            Subject = $"Welcome {userName}!",
            BodyHtml = await GetTemplateContentAsync("WelcomeEmail", variables),
            Type = EmailType.WelcomeEmail,
            TenantId = tenantId
        };

        return await SendEmailAsync(message);
    }

    private async Task<string> GetTemplateContentAsync(string templateName, Dictionary<string, string> variables)
    {
        // Use compiled templates from dist/ folder (compiled from MJML source)
        var templatePath = Path.Combine("Data", "EmailTemplates", "dist", $"{templateName}.html");

        if (!File.Exists(templatePath))
        {
            _logger.LogWarning("Email template not found: {TemplatePath}. Make sure templates are compiled from MJML.", templatePath);
            return BuildSimpleEmailTemplate(variables);
        }

        var content = await File.ReadAllTextAsync(templatePath);
        return ReplaceVariables(content, variables);
    }

    private static string BuildSimpleEmailTemplate(Dictionary<string, string> variables)
    {
        var body = "<html><body><h1>Email Content</h1>";
        foreach (var variable in variables)
        {
            body += $"<p>{variable.Key}: {variable.Value}</p>";
        }
        body += "</body></html>";
        return body;
    }

    private static string ReplaceVariables(string template, Dictionary<string, string> variables)
    {
        foreach (var variable in variables)
        {
            template = template.Replace(variable.Key, variable.Value);
        }
        return template;
    }

    private static string StripHtml(string html)
    {
        return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
    }
}
