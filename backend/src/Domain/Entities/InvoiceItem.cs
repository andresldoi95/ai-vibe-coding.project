using SaaS.Domain.Common;

namespace SaaS.Domain.Entities;

public class InvoiceItem : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public Guid ProductId { get; set; }

    // Denormalized product data (preserved history)
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public Guid TaxRateId { get; set; }
    public decimal TaxRate { get; set; } // Denormalized rate value

    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    // Navigation properties
    public virtual Invoice Invoice { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public virtual TaxRate TaxRateEntity { get; set; } = null!;
}
