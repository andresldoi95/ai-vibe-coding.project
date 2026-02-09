using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Application.Services;
using SaaS.Domain.Entities;

namespace SaaS.Application.Features.StockMovements.Commands.CreateStockMovement;

/// <summary>
/// Handler for creating a new stock movement
/// </summary>
public class CreateStockMovementCommandHandler : IRequestHandler<CreateStockMovementCommand, Result<StockMovementDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IStockLevelService _stockLevelService;
    private readonly ILogger<CreateStockMovementCommandHandler> _logger;

    public CreateStockMovementCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        IStockLevelService stockLevelService,
        ILogger<CreateStockMovementCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _stockLevelService = stockLevelService;
        _logger = logger;
    }

    public async Task<Result<StockMovementDto>> Handle(CreateStockMovementCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<StockMovementDto>.Failure("Tenant context is required");
            }

            var tenantId = _tenantContext.TenantId.Value;

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
            Warehouse? destinationWarehouse = null;
            if (request.DestinationWarehouseId.HasValue)
            {
                destinationWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(request.DestinationWarehouseId.Value, cancellationToken);
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
            var movementDate = request.MovementDate ?? DateTime.UtcNow;
            if (movementDate.Kind == DateTimeKind.Unspecified)
            {
                movementDate = DateTime.SpecifyKind(movementDate, DateTimeKind.Utc);
            }
            else if (movementDate.Kind == DateTimeKind.Local)
            {
                movementDate = movementDate.ToUniversalTime();
            }

            // Create stock movement entity
            var stockMovement = new StockMovement
            {
                TenantId = tenantId,
                MovementType = request.MovementType,
                ProductId = request.ProductId,
                WarehouseId = request.WarehouseId,
                DestinationWarehouseId = request.DestinationWarehouseId,
                Quantity = request.Quantity,
                UnitCost = request.UnitCost,
                TotalCost = totalCost,
                Reference = request.Reference,
                Notes = request.Notes,
                MovementDate = movementDate
            };

            await _unitOfWork.StockMovements.AddAsync(stockMovement, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Update warehouse inventory levels
            await _stockLevelService.UpdateStockLevelsAsync(stockMovement, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Stock movement {Id} created for product {ProductId} in warehouse {WarehouseId}",
                stockMovement.Id,
                stockMovement.ProductId,
                stockMovement.WarehouseId);

            // Load related entities for DTO
            var createdMovement = await _unitOfWork.StockMovements.GetByIdWithDetailsAsync(stockMovement.Id, tenantId);
            if (createdMovement == null)
            {
                return Result<StockMovementDto>.Failure("Failed to retrieve created stock movement");
            }

            // Map to DTO
            var dto = MapToDto(createdMovement);

            return Result<StockMovementDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating stock movement");
            return Result<StockMovementDto>.Failure("An error occurred while creating the stock movement");
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
