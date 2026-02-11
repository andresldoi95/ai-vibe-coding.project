namespace SaaS.Application.DTOs;

public class InvoiceConfigurationDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string EstablishmentCode { get; set; } = string.Empty;
    public string EmissionPointCode { get; set; } = string.Empty;
    public int NextSequentialNumber { get; set; }
    public Guid? DefaultTaxRateId { get; set; }
    public Guid? DefaultWarehouseId { get; set; }
    public int DueDays { get; set; }
    public bool RequireCustomerTaxId { get; set; }

    // Navigation properties (optional)
    public string? DefaultTaxRateName { get; set; }
    public string? DefaultWarehouseName { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
