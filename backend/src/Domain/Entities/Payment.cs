using SaaS.Domain.Common;
using SaaS.Domain.Enums;

namespace SaaS.Domain.Entities;

/// <summary>
/// Represents a payment transaction for an invoice
/// </summary>
public class Payment : TenantEntity
{
    /// <summary>
    /// Foreign key to the associated invoice
    /// </summary>
    public Guid InvoiceId { get; set; }

    /// <summary>
    /// Navigation property to the invoice
    /// </summary>
    public Invoice Invoice { get; set; } = null!;

    /// <summary>
    /// Payment amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Date when the payment was made or is scheduled
    /// </summary>
    public DateTime PaymentDate { get; set; }

    /// <summary>
    /// Payment method used (SRI compliant for Ecuador)
    /// </summary>
    public SriPaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// Current status of the payment
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Optional transaction ID for external payment references
    /// </summary>
    public string? TransactionId { get; set; }

    /// <summary>
    /// Optional notes or comments about the payment
    /// </summary>
    public string? Notes { get; set; }
}
