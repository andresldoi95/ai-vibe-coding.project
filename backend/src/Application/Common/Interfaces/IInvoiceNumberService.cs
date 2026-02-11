namespace SaaS.Application.Common.Interfaces;

/// <summary>
/// Service for generating invoice numbers
/// </summary>
public interface IInvoiceNumberService
{
    /// <summary>
    /// Generate next invoice number for tenant (thread-safe)
    /// </summary>
    Task<string> GenerateNextInvoiceNumberAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate invoice number format
    /// </summary>
    bool ValidateFormat(string invoiceNumber);
}
