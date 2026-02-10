using SaaS.Domain.Common;

namespace SaaS.Domain.Entities;

/// <summary>
/// Product entity representing items in the inventory management system
/// </summary>
public class Product : TenantEntity
{
    // Basic Information
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SKU { get; set; } = string.Empty;

    // Category/Classification
    public string? Category { get; set; }
    public string? Brand { get; set; }

    // Pricing
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }

    // Inventory
    public int MinimumStockLevel { get; set; }

    // Physical Properties
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }

    // Status
    public bool IsActive { get; set; } = true;
}
