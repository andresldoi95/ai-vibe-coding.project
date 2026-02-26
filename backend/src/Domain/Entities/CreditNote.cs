using SaaS.Domain.Common;
using SaaS.Domain.Enums;

namespace SaaS.Domain.Entities;

public class CreditNote : TenantEntity
{
    public string CreditNoteNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public DateTime IssueDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
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

    // SRI Ecuador fields
    public Guid? EmissionPointId { get; set; }
    public DocumentType DocumentType { get; set; } = DocumentType.CreditNote;
    public string? AccessKey { get; set; }
    public SriPaymentMethod PaymentMethod { get; set; } = SriPaymentMethod.Cash;
    public string? XmlFilePath { get; set; }
    public string? SignedXmlFilePath { get; set; }
    public string? RideFilePath { get; set; }
    public SriEnvironment Environment { get; set; } = SriEnvironment.Test;
    public string? SriAuthorization { get; set; }
    public DateTime? AuthorizationDate { get; set; }

    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual EmissionPoint? EmissionPoint { get; set; }
    public virtual Invoice OriginalInvoice { get; set; } = null!;
    public virtual ICollection<CreditNoteItem> Items { get; set; } = new List<CreditNoteItem>();

    // Computed property
    public bool IsEditable => Status == InvoiceStatus.Draft && !IsDeleted;
}
