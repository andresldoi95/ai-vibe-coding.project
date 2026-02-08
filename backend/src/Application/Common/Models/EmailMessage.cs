using SaaS.Domain.Enums;

namespace SaaS.Application.Common.Models;

public class EmailMessage
{
    public string To { get; set; } = string.Empty;
    public string? Cc { get; set; }
    public string? Bcc { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? BodyText { get; set; }
    public EmailType Type { get; set; } = EmailType.Custom;
    public Guid? TenantId { get; set; }
    public Guid? UserId { get; set; }
}
