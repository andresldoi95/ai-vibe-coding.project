namespace SaaS.Domain.Enums;

/// <summary>
/// Represents the status of a payment transaction
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment has been initiated but not yet confirmed
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Payment has been successfully completed
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Payment has been voided/cancelled
    /// </summary>
    Voided = 3
}
