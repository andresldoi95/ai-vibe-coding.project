using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace SaaS.Application.Services;

/// <summary>
/// Service for managing warehouse inventory stock levels
/// Updates inventory quantities based on stock movements
/// </summary>
public class StockLevelService : IStockLevelService
{
    private readonly IUnitOfWork _unitOfWork;

    public StockLevelService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Update warehouse inventory levels based on a stock movement
    /// </summary>
    public async Task UpdateStockLevelsAsync(StockMovement movement, CancellationToken cancellationToken = default)
    {
        // Calculate quantity delta based on movement type
        var (sourceQuantityDelta, destinationQuantityDelta) = CalculateQuantityDeltas(movement);

        // Update source warehouse inventory
        if (sourceQuantityDelta != 0)
        {
            await _unitOfWork.WarehouseInventory.UpsertAsync(
                movement.ProductId,
                movement.WarehouseId,
                movement.TenantId,
                sourceQuantityDelta,
                movement.MovementDate,
                cancellationToken);
        }

        // Update destination warehouse inventory (for transfers only)
        if (movement.DestinationWarehouseId.HasValue && destinationQuantityDelta != 0)
        {
            await _unitOfWork.WarehouseInventory.UpsertAsync(
                movement.ProductId,
                movement.DestinationWarehouseId.Value,
                movement.TenantId,
                destinationQuantityDelta,
                movement.MovementDate,
                cancellationToken);
        }
    }

    /// <summary>
    /// Reverse a stock movement's effect on inventory (for updates/deletes)
    /// </summary>
    public async Task ReverseStockLevelsAsync(StockMovement movement, CancellationToken cancellationToken = default)
    {
        // Calculate the opposite effect
        var (sourceQuantityDelta, destinationQuantityDelta) = CalculateQuantityDeltas(movement);

        // Reverse source warehouse inventory
        if (sourceQuantityDelta != 0)
        {
            await _unitOfWork.WarehouseInventory.UpsertAsync(
                movement.ProductId,
                movement.WarehouseId,
                movement.TenantId,
                -sourceQuantityDelta, // Negate the delta to reverse
                movement.MovementDate,
                cancellationToken);
        }

        // Reverse destination warehouse inventory (for transfers only)
        if (movement.DestinationWarehouseId.HasValue && destinationQuantityDelta != 0)
        {
            await _unitOfWork.WarehouseInventory.UpsertAsync(
                movement.ProductId,
                movement.DestinationWarehouseId.Value,
                movement.TenantId,
                -destinationQuantityDelta, // Negate the delta to reverse
                movement.MovementDate,
                cancellationToken);
        }
    }

    /// <summary>
    /// Calculate quantity deltas for source and destination warehouses
    /// </summary>
    private (int sourceQuantityDelta, int destinationQuantityDelta) CalculateQuantityDeltas(StockMovement movement)
    {
        int sourceQuantityDelta = 0;
        int destinationQuantityDelta = 0;

        switch (movement.MovementType)
        {
            case MovementType.InitialInventory:
            case MovementType.Purchase:
            case MovementType.Return:
                // Add stock to warehouse
                sourceQuantityDelta = Math.Abs(movement.Quantity);
                break;

            case MovementType.Sale:
                // Remove stock from warehouse
                sourceQuantityDelta = -Math.Abs(movement.Quantity);
                break;

            case MovementType.Transfer:
                // Remove from source, add to destination
                sourceQuantityDelta = -Math.Abs(movement.Quantity);
                destinationQuantityDelta = Math.Abs(movement.Quantity);
                break;

            case MovementType.Adjustment:
                // Use quantity as-is (can be positive or negative)
                sourceQuantityDelta = movement.Quantity;
                break;

            default:
                throw new InvalidOperationException($"Unknown movement type: {movement.MovementType}");
        }

        return (sourceQuantityDelta, destinationQuantityDelta);
    }
}

/// <summary>
/// Interface for stock level service
/// </summary>
public interface IStockLevelService
{
    Task UpdateStockLevelsAsync(StockMovement movement, CancellationToken cancellationToken = default);
    Task ReverseStockLevelsAsync(StockMovement movement, CancellationToken cancellationToken = default);
}
