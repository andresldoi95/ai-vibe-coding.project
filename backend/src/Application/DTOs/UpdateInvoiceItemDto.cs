namespace SaaS.Application.DTOs;

public class UpdateInvoiceItemDto
{
    public Guid? Id { get; set; } // Present if updating existing item, null if adding new
    public Guid ProductId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public Guid TaxRateId { get; set; }
}
