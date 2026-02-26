using SaaS.Domain.Enums;

namespace SaaS.Application.DTOs;

public class CreditNoteDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string CreditNoteNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }

    // Original invoice reference
    public Guid OriginalInvoiceId { get; set; }
    public string OriginalInvoiceNumber { get; set; } = string.Empty;
    public DateTime OriginalInvoiceDate { get; set; }

    // Credit note specific fields
    public string Reason { get; set; } = string.Empty;
    public decimal ValueModification { get; set; }
    public bool IsPhysicalReturn { get; set; }

    // Emission Point
    public Guid? EmissionPointId { get; set; }
    public string? EmissionPointCode { get; set; }
    public string? EmissionPointName { get; set; }
    public string? EstablishmentCode { get; set; }

    // SRI fields
    public DocumentType DocumentType { get; set; }
    public string? AccessKey { get; set; }
    public SriPaymentMethod PaymentMethod { get; set; }
    public string? XmlFilePath { get; set; }
    public string? SignedXmlFilePath { get; set; }
    public string? RideFilePath { get; set; }
    public SriEnvironment Environment { get; set; }
    public string? SriAuthorization { get; set; }
    public DateTime? AuthorizationDate { get; set; }

    // Items
    public List<CreditNoteItemDto> Items { get; set; } = new();

    // Computed
    public bool IsEditable { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
