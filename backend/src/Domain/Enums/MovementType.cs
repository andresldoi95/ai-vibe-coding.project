namespace SaaS.Domain.Enums;

/// <summary>
/// Represents the type of stock movement operation
/// </summary>
public enum MovementType
{
    /// <summary>
    /// Initial inventory setup when adding product to warehouse for first time
    /// </summary>
    InitialInventory = 0,
    
    /// <summary>
    /// Stock added from purchase order
    /// </summary>
    Purchase = 1,
    
    /// <summary>
    /// Stock removed due to sale
    /// </summary>
    Sale = 2,
    
    /// <summary>
    /// Stock transferred between warehouses
    /// </summary>
    Transfer = 3,
    
    /// <summary>
    /// Manual adjustment (corrections, damage, loss, etc.)
    /// </summary>
    Adjustment = 4,
    
    /// <summary>
    /// Stock returned from customer
    /// </summary>
    Return = 5
}
