using SaaS.Domain.Common;

namespace SaaS.Domain.Entities;

/// <summary>
/// Country entity with ISO 3166-1 standard codes.
/// Global entity (not tenant-scoped) - shared across all tenants.
/// </summary>
public class Country : BaseEntity
{
    /// <summary>
    /// ISO 3166-1 alpha-2 country code (e.g., "US", "EC", "DE")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Country name in English (e.g., "United States", "Ecuador", "Germany")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// ISO 3166-1 alpha-3 country code (e.g., "USA", "ECU", "DEU")
    /// </summary>
    public string? Alpha3Code { get; set; }

    /// <summary>
    /// ISO 3166-1 numeric country code (e.g., "840" for USA)
    /// </summary>
    public string? NumericCode { get; set; }

    /// <summary>
    /// Whether this country is active and available for selection
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<TaxRate> TaxRates { get; set; } = new List<TaxRate>();
    public ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
}
