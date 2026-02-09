using SaaS.Domain.Common;

namespace SaaS.Domain.Entities;

/// <summary>
/// Warehouse Inventory entity representing stock levels for products in specific warehouses
/// Tracks real-time inventory quantities and reserved stock
/// </summary>
public class WarehouseInventory : TenantEntity
{
    /// <summary>
    /// Product being tracked
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Warehouse where the product is stored
    /// </summary>
    public Guid WarehouseId { get; set; }

    /// <summary>
    /// Total physical quantity in warehouse
    /// Calculated from stock movements
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Quantity reserved for pending orders/transfers
    /// Not yet implemented, reserved for future use
    /// </summary>
    public int ReservedQuantity { get; set; }

    /// <summary>
    /// Available quantity for new orders
    /// Calculated as: Quantity - ReservedQuantity
    /// </summary>
    public int AvailableQuantity => Quantity - ReservedQuantity;

    /// <summary>
    /// Date of the last stock movement affecting this inventory
    /// </summary>
    public DateTime? LastMovementDate { get; set; }

    // Navigation Properties

    /// <summary>
    /// The product associated with this inventory record
    /// </summary>
    public virtual Product Product { get; set; } = null!;

    /// <summary>
    /// The warehouse where the product is stored
    /// </summary>
    public virtual Warehouse Warehouse { get; set; } = null!;
}
