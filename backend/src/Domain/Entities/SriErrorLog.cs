using SaaS.Domain.Common;

namespace SaaS.Domain.Entities;

/// <summary>
/// Logs errors that occur during SRI electronic invoicing operations.
/// Used for debugging, compliance tracking, and error analysis.
/// </summary>
public class SriErrorLog : TenantEntity
{
    /// <summary>
    /// Reference to the invoice that encountered the error
    /// </summary>
    public Guid InvoiceId { get; set; }

    /// <summary>
    /// Type of operation that failed (e.g., "GenerateXml", "SignDocument", "SubmitToSRI", "CheckAuthorization", "GenerateRIDE")
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// SRI error code (if provided by SRI web services)
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Human-readable error message
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Full exception stack trace for debugging
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Additional context data (JSON serialized)
    /// </summary>
    public string? AdditionalData { get; set; }

    /// <summary>
    /// When the error occurred (UTC)
    /// </summary>
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the operation was retried automatically
    /// </summary>
    public bool WasRetried { get; set; }

    /// <summary>
    /// If retried, whether the retry succeeded
    /// </summary>
    public bool? RetrySucceeded { get; set; }

    // Navigation properties
    public virtual Invoice Invoice { get; set; } = null!;
}
