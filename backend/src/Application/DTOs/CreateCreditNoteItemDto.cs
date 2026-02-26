namespace SaaS.Application.DTOs;

public class CreateCreditNoteItemDto
{
    public Guid ProductId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public Guid TaxRateId { get; set; }
}
