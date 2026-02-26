using SaaS.Domain.Common;
using SaaS.Domain.Enums;

namespace SaaS.Domain.Entities;

/// <summary>
/// Represents a point of emission within an establishment.
/// Each emission point can generate electronic documents with its own sequential numbering.
/// Required by SRI Ecuador for electronic invoice generation.
/// </summary>
public class EmissionPoint : TenantEntity
{
    /// <summary>
    /// 3-digit emission point code (001-999)
    /// </summary>
    public string EmissionPointCode { get; set; } = string.Empty;

    /// <summary>
    /// Emission point name/description
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether this emission point is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Next sequential number for invoices
    /// </summary>
    public int InvoiceSequence { get; set; } = 1;

    /// <summary>
    /// Next sequential number for credit notes
    /// </summary>
    public int CreditNoteSequence { get; set; } = 1;

    /// <summary>
    /// Next sequential number for debit notes
    /// </summary>
    public int DebitNoteSequence { get; set; } = 1;

    /// <summary>
    /// Next sequential number for retention receipts
    /// </summary>
    public int RetentionSequence { get; set; } = 1;

    /// <summary>
    /// Establishment that owns this emission point
    /// </summary>
    public Guid EstablishmentId { get; set; }
    public Establishment Establishment { get; set; } = null!;

    /// <summary>
    /// Invoices generated at this emission point
    /// </summary>
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    /// <summary>
    /// Credit notes generated at this emission point
    /// </summary>
    public ICollection<CreditNote> CreditNotes { get; set; } = new List<CreditNote>();

    /// <summary>
    /// Validates the emission point code format (must be exactly 3 digits)
    /// </summary>
    public bool IsValidCode()
    {
        if (string.IsNullOrWhiteSpace(EmissionPointCode))
            return false;

        if (EmissionPointCode.Length != 3)
            return false;

        if (!EmissionPointCode.All(char.IsDigit))
            return false;

        var code = int.Parse(EmissionPointCode);
        return code >= 1 && code <= 999;
    }

    /// <summary>
    /// Gets the next sequential number for a document type and increments it
    /// This method should be called within a transaction
    /// </summary>
    public int GetNextSequential(DocumentType documentType)
    {
        var currentSequential = documentType switch
        {
            DocumentType.Invoice => InvoiceSequence++,
            DocumentType.CreditNote => CreditNoteSequence++,
            DocumentType.DebitNote => DebitNoteSequence++,
            DocumentType.Retention => RetentionSequence++,
            _ => throw new ArgumentException($"Unsupported document type: {documentType}", nameof(documentType))
        };

        return currentSequential;
    }

    /// <summary>
    /// Gets current sequential number without incrementing
    /// </summary>
    public int GetCurrentSequential(DocumentType documentType)
    {
        return documentType switch
        {
            DocumentType.Invoice => InvoiceSequence,
            DocumentType.CreditNote => CreditNoteSequence,
            DocumentType.DebitNote => DebitNoteSequence,
            DocumentType.Retention => RetentionSequence,
            _ => throw new ArgumentException($"Unsupported document type: {documentType}", nameof(documentType))
        };
    }
}
