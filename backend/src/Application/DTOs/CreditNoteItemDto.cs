namespace SaaS.Application.DTOs;

public class CreditNoteItemDto
{
    public Guid Id { get; set; }
    public Guid CreditNoteId { get; set; }
    public Guid ProductId { get; set; }

    // Denormalized product data
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public Guid TaxRateId { get; set; }
    public decimal TaxRate { get; set; }

    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
}
