using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Entities;

namespace SaaS.Application.Features.StockMovements.Queries.GetAllStockMovements;

/// <summary>
/// Handler for getting all stock movements
/// </summary>
public class GetAllStockMovementsQueryHandler : IRequestHandler<GetAllStockMovementsQuery, Result<List<StockMovementDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetAllStockMovementsQueryHandler> _logger;

    public GetAllStockMovementsQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetAllStockMovementsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<List<StockMovementDto>>> Handle(GetAllStockMovementsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<List<StockMovementDto>>.Failure("Tenant context is required");
            }

            var tenantId = _tenantContext.TenantId.Value;

            var stockMovements = await _unitOfWork.StockMovements.GetAllForTenantAsync(tenantId);

            var dtos = stockMovements.Select(MapToDto).ToList();

            _logger.LogInformation(
                "Retrieved {Count} stock movements for tenant {TenantId}",
                dtos.Count,
                tenantId);

            return Result<List<StockMovementDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stock movements");
            return Result<List<StockMovementDto>>.Failure("An error occurred while retrieving stock movements");
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
