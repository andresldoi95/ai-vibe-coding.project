using SaaS.Domain.Common;
using SaaS.Domain.Enums;

namespace SaaS.Domain.Entities;

public class EmailLog : TenantEntity
{
    public Guid? UserId { get; set; }
    public string To { get; set; } = string.Empty;
    public string? Cc { get; set; }
    public string? Bcc { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public EmailType Type { get; set; }
    public EmailStatus Status { get; set; } = EmailStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public int RetryCount { get; set; } = 0;

    // Navigation
    public User? User { get; set; }
}
