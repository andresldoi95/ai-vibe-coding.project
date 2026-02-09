using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Entities;

namespace SaaS.Application.Features.StockMovements.Queries.GetStockMovementById;

/// <summary>
/// Handler for getting a stock movement by ID
/// </summary>
public class GetStockMovementByIdQueryHandler : IRequestHandler<GetStockMovementByIdQuery, Result<StockMovementDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetStockMovementByIdQueryHandler> _logger;

    public GetStockMovementByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetStockMovementByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<StockMovementDto>> Handle(GetStockMovementByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<StockMovementDto>.Failure("Tenant context is required");
            }

            var tenantId = _tenantContext.TenantId.Value;

            var stockMovement = await _unitOfWork.StockMovements.GetByIdWithDetailsAsync(request.Id, tenantId);
            if (stockMovement == null || stockMovement.TenantId != tenantId)
            {
                return Result<StockMovementDto>.Failure("Stock movement not found");
            }

            var dto = MapToDto(stockMovement);

            _logger.LogInformation(
                "Retrieved stock movement {Id} for tenant {TenantId}",
                request.Id,
                tenantId);

            return Result<StockMovementDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stock movement {Id}", request.Id);
            return Result<StockMovementDto>.Failure("An error occurred while retrieving the stock movement");
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
