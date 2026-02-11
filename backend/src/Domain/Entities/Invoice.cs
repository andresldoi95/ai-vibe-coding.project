using SaaS.Domain.Common;
using SaaS.Domain.Enums;

namespace SaaS.Domain.Entities;

public class Invoice : TenantEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public Guid? WarehouseId { get; set; }

    // Ecuador SRI fields
    public string? SriAuthorization { get; set; }
    public DateTime? AuthorizationDate { get; set; }
    public DateTime? PaidDate { get; set; }

    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual Warehouse? Warehouse { get; set; }
    public virtual ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

    // Computed property
    public bool IsEditable => Status == InvoiceStatus.Draft && !IsDeleted;
}
