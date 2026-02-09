using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.StockMovements.Commands.UpdateStockMovement;

/// <summary>
/// Command to update an existing stock movement
/// </summary>
public record UpdateStockMovementCommand : IRequest<Result<StockMovementDto>>
{
    public Guid Id { get; init; }
    public MovementType MovementType { get; init; }
    public Guid ProductId { get; init; }
    public Guid WarehouseId { get; init; }
    public Guid? DestinationWarehouseId { get; init; }
    public int Quantity { get; init; }
    public decimal? UnitCost { get; init; }
    public decimal? TotalCost { get; init; }
    public string? Reference { get; init; }
    public string? Notes { get; init; }
    public DateTime MovementDate { get; init; }
}
