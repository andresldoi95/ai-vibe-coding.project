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
    
    /// <summary>
    /// DEPRECATED: This field is no longer used. Stock levels are calculated from WarehouseInventory.
    /// Use InitialQuantity/InitialWarehouseId when creating products to set initial stock.
    /// Use StockMovement to change inventory levels.
    /// This field is kept for backward compatibility with existing database schema.
    /// </summary>
    [Obsolete("Use WarehouseInventory for stock tracking. This field is deprecated and should not be updated.")]
    public int? CurrentStockLevel { get; set; }

    // Physical Properties
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }

    // Status
    public bool IsActive { get; set; } = true;
}
