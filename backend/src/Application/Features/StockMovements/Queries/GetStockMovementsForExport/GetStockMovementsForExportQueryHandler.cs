using MediatR;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.StockMovements.Queries.GetStockMovementsForExport;

/// <summary>
/// Handler for GetStockMovementsForExportQuery
/// Retrieves stock movements with optional filtering by brand, category, warehouse, and date range
/// </summary>
public class GetStockMovementsForExportQueryHandler
    : IRequestHandler<GetStockMovementsForExportQuery, Result<List<StockMovementExportDto>>>
{
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly ITenantContext _tenantContext;

    public GetStockMovementsForExportQueryHandler(
        IStockMovementRepository stockMovementRepository,
        ITenantContext tenantContext)
    {
        _stockMovementRepository = stockMovementRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<List<StockMovementExportDto>>> Handle(
        GetStockMovementsForExportQuery request,
        CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return Result<List<StockMovementExportDto>>.Failure("Tenant not found");
        }

        var tenantId = _tenantContext.TenantId.Value;

        var stockMovements = await _stockMovementRepository.GetForExportAsync(
            tenantId,
            request.Brand,
            request.Category,
            request.WarehouseId,
            request.FromDate,
            request.ToDate,
            cancellationToken);

        var movements = stockMovements.Select(sm => new StockMovementExportDto
        {
            MovementDate = sm.MovementDate.ToString("yyyy-MM-dd HH:mm:ss"),
            MovementType = sm.MovementType.ToString(),
            ProductCode = sm.Product.Code,
            ProductName = sm.Product.Name,
            Brand = sm.Product.Brand,
            Category = sm.Product.Category,
            WarehouseCode = sm.Warehouse.Code,
            WarehouseName = sm.Warehouse.Name,
            DestinationWarehouseCode = sm.DestinationWarehouse?.Code,
            DestinationWarehouseName = sm.DestinationWarehouse?.Name,
            Quantity = sm.Quantity,
            UnitCost = sm.UnitCost,
            TotalCost = sm.TotalCost,
            Reference = sm.Reference,
            Notes = sm.Notes
        }).ToList();

        return Result<List<StockMovementExportDto>>.Success(movements);
    }
}
