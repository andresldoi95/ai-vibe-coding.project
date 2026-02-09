using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Application.Services;
using SaaS.Domain.Entities;

namespace SaaS.Application.Features.StockMovements.Commands.UpdateStockMovement;

/// <summary>
/// Handler for updating an existing stock movement
/// </summary>
public class UpdateStockMovementCommandHandler : IRequestHandler<UpdateStockMovementCommand, Result<StockMovementDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IStockLevelService _stockLevelService;
    private readonly ILogger<UpdateStockMovementCommandHandler> _logger;

    public UpdateStockMovementCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        IStockLevelService stockLevelService,
        ILogger<UpdateStockMovementCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _stockLevelService = stockLevelService;
        _logger = logger;
    }

    public async Task<Result<StockMovementDto>> Handle(UpdateStockMovementCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<StockMovementDto>.Failure("Tenant context is required");
            }

            var tenantId = _tenantContext.TenantId.Value;

            // Get existing stock movement
            var stockMovement = await _unitOfWork.StockMovements.GetByIdAsync(request.Id, cancellationToken);
            if (stockMovement == null || stockMovement.TenantId != tenantId)
            {
                return Result<StockMovementDto>.Failure("Stock movement not found");
            }

            // Validate Product exists and belongs to tenant
            var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null || product.TenantId != tenantId)
            {
                return Result<StockMovementDto>.Failure("Product not found");
            }

            // Validate Warehouse exists and belongs to tenant
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(request.WarehouseId, cancellationToken);
            if (warehouse == null || warehouse.TenantId != tenantId)
            {
                return Result<StockMovementDto>.Failure("Warehouse not found");
            }

            // Validate Destination Warehouse if provided
            if (request.DestinationWarehouseId.HasValue)
            {
                var destinationWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(request.DestinationWarehouseId.Value, cancellationToken);
                if (destinationWarehouse == null || destinationWarehouse.TenantId != tenantId)
                {
                    return Result<StockMovementDto>.Failure("Destination warehouse not found");
                }
            }

            // Calculate total cost if not provided but unit cost is available
            var totalCost = request.TotalCost;
            if (!totalCost.HasValue && request.UnitCost.HasValue)
            {
                totalCost = request.UnitCost.Value * Math.Abs(request.Quantity);
            }

            // Ensure MovementDate is UTC
            var movementDate = request.MovementDate;
            if (movementDate.Kind == DateTimeKind.Unspecified)
            {
                movementDate = DateTime.SpecifyKind(movementDate, DateTimeKind.Utc);
            }
            else if (movementDate.Kind == DateTimeKind.Local)
            {
                movementDate = movementDate.ToUniversalTime();
            }

            // Store old values for reversing stock levels
            var oldMovement = new StockMovement
            {
                ProductId = stockMovement.ProductId,
                WarehouseId = stockMovement.WarehouseId,
                DestinationWarehouseId = stockMovement.DestinationWarehouseId,
                MovementType = stockMovement.MovementType,
                Quantity = stockMovement.Quantity,
                MovementDate = stockMovement.MovementDate,
                TenantId = stockMovement.TenantId
            };

            // Reverse old stock levels
            await _stockLevelService.ReverseStockLevelsAsync(oldMovement, cancellationToken);

            // Update stock movement
            stockMovement.MovementType = request.MovementType;
            stockMovement.ProductId = request.ProductId;
            stockMovement.WarehouseId = request.WarehouseId;
            stockMovement.DestinationWarehouseId = request.DestinationWarehouseId;
            stockMovement.Quantity = request.Quantity;
            stockMovement.UnitCost = request.UnitCost;
            stockMovement.TotalCost = totalCost;
            stockMovement.Reference = request.Reference;
            stockMovement.Notes = request.Notes;
            stockMovement.MovementDate = movementDate;

            // Apply new stock levels
            await _stockLevelService.UpdateStockLevelsAsync(stockMovement, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Stock movement {Id} updated for tenant {TenantId}",
                stockMovement.Id,
                tenantId);

            // Load related entities for DTO
            var updatedMovement = await _unitOfWork.StockMovements.GetByIdWithDetailsAsync(stockMovement.Id, tenantId);
            if (updatedMovement == null)
            {
                return Result<StockMovementDto>.Failure("Failed to retrieve updated stock movement");
            }

            // Map to DTO
            var dto = MapToDto(updatedMovement);

            return Result<StockMovementDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock movement");
            return Result<StockMovementDto>.Failure("An error occurred while updating the stock movement");
        }
    }

    private static StockMovementDto MapToDto(StockMovement movement)
    {
        return new StockMovementDto
        {
            Id = movement.Id,
            MovementType = movement.MovementType,
            MovementTypeName = movement.MovementType.ToString(),
            ProductId = movement.ProductId,
            ProductName = movement.Product?.Name ?? string.Empty,
            ProductCode = movement.Product?.Code ?? string.Empty,
            WarehouseId = movement.WarehouseId,
            WarehouseName = movement.Warehouse?.Name ?? string.Empty,
            WarehouseCode = movement.Warehouse?.Code ?? string.Empty,
            DestinationWarehouseId = movement.DestinationWarehouseId,
            DestinationWarehouseName = movement.DestinationWarehouse?.Name,
            DestinationWarehouseCode = movement.DestinationWarehouse?.Code,
            Quantity = movement.Quantity,
            UnitCost = movement.UnitCost,
            TotalCost = movement.TotalCost,
            Reference = movement.Reference,
            Notes = movement.Notes,
            MovementDate = movement.MovementDate,
            CreatedAt = movement.CreatedAt,
            UpdatedAt = movement.UpdatedAt
        };
    }
}
