namespace SaaS.Application.DTOs;

/// <summary>
/// DTO for stock movement export
/// Contains detailed stock movement information with product and warehouse details
/// </summary>
public class StockMovementExportDto
{
    public string MovementDate { get; set; } = string.Empty;
    public string MovementType { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Category { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string? DestinationWarehouseCode { get; set; }
    public string? DestinationWarehouseName { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? TotalCost { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}
