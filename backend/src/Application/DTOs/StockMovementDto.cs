using SaaS.Domain.Enums;

namespace SaaS.Application.DTOs;

/// <summary>
/// Data Transfer Object for Stock Movement
/// </summary>
public class StockMovementDto
{
    public Guid Id { get; set; }
    public MovementType MovementType { get; set; }
    public string MovementTypeName { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public Guid? DestinationWarehouseId { get; set; }
    public string? DestinationWarehouseName { get; set; }
    public string? DestinationWarehouseCode { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? TotalCost { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public DateTime MovementDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
