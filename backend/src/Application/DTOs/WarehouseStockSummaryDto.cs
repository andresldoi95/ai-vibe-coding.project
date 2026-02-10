namespace SaaS.Application.DTOs;

/// <summary>
/// DTO for warehouse stock summary export
/// Contains aggregated stock information per warehouse and product
/// </summary>
public class WarehouseStockSummaryDto
{
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Category { get; set; }
    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public string? LastMovementDate { get; set; }
}
