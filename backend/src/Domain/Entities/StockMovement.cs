using SaaS.Domain.Common;
using SaaS.Domain.Enums;

namespace SaaS.Domain.Entities;

/// <summary>
/// Stock Movement entity representing inventory transactions and transfers
/// Tracks all inventory changes including purchases, sales, transfers, and adjustments
/// </summary>
public class StockMovement : TenantEntity
{
    /// <summary>
    /// Type of movement operation
    /// </summary>
    public MovementType MovementType { get; set; }
    
    /// <summary>
    /// Product being moved (Required)
    /// </summary>
    public Guid ProductId { get; set; }
    
    /// <summary>
    /// Source warehouse for the movement (Required)
    /// For incoming stock, this is the destination warehouse
    /// </summary>
    public Guid WarehouseId { get; set; }
    
    /// <summary>
    /// Destination warehouse for transfer operations (Optional)
    /// Only used when MovementType = Transfer
    /// </summary>
    public Guid? DestinationWarehouseId { get; set; }
    
    /// <summary>
    /// Quantity of items moved
    /// Positive for incoming stock (Purchase, Return, InitialInventory)
    /// Negative for outgoing stock (Sale, Transfer out)
    /// Can be positive or negative for Adjustment
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Cost per unit (Optional)
    /// Used for purchase orders and initial inventory
    /// </summary>
    public decimal? UnitCost { get; set; }
    
    /// <summary>
    /// Total cost of the movement (Optional)
    /// Calculated as Quantity * UnitCost if both are provided
    /// </summary>
    public decimal? TotalCost { get; set; }
    
    /// <summary>
    /// External reference (Optional)
    /// Could be invoice number, PO number, order reference, etc.
    /// </summary>
    public string? Reference { get; set; }
    
    /// <summary>
    /// Additional notes or comments about the movement (Optional)
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Date and time when the movement occurred
    /// Defaults to current UTC time
    /// </summary>
    public DateTime MovementDate { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    
    /// <summary>
    /// The product associated with this movement
    /// </summary>
    public virtual Product Product { get; set; } = null!;
    
    /// <summary>
    /// The source/primary warehouse for this movement
    /// </summary>
    public virtual Warehouse Warehouse { get; set; } = null!;
    
    /// <summary>
    /// The destination warehouse (only for transfers)
    /// </summary>
    public virtual Warehouse? DestinationWarehouse { get; set; }
}
