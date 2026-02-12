using SaaS.Domain.Common;

namespace SaaS.Domain.Entities;

public class TaxRate : TenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }

    // Country relationship (nullable - tax rate may apply to all countries)
    public Guid? CountryId { get; set; }
    public Country? Country { get; set; }
}
