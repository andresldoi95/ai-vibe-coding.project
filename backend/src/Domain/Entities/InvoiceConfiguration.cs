using SaaS.Domain.Common;

namespace SaaS.Domain.Entities;

public class InvoiceConfiguration : TenantEntity
{
    public string EstablishmentCode { get; set; } = "001";
    public string EmissionPointCode { get; set; } = "001";
    public int NextSequentialNumber { get; set; } = 1;
    public Guid? DefaultTaxRateId { get; set; }
    public Guid? DefaultWarehouseId { get; set; }
    public int DueDays { get; set; } = 30;
    public bool RequireCustomerTaxId { get; set; }

    // Navigation properties
    public virtual TaxRate? DefaultTaxRate { get; set; }
    public virtual Warehouse? DefaultWarehouse { get; set; }
}
