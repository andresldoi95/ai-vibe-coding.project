using MediatR;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Warehouses.Queries.GetWarehouseStockSummary;

/// <summary>
/// Handler for GetWarehouseStockSummaryQuery
/// Retrieves warehouse stock summary with product details
/// </summary>
public class GetWarehouseStockSummaryQueryHandler
    : IRequestHandler<GetWarehouseStockSummaryQuery, Result<List<WarehouseStockSummaryDto>>>
{
    private readonly IWarehouseInventoryRepository _warehouseInventoryRepository;
    private readonly ITenantContext _tenantContext;

    public GetWarehouseStockSummaryQueryHandler(
        IWarehouseInventoryRepository warehouseInventoryRepository,
        ITenantContext tenantContext)
    {
        _warehouseInventoryRepository = warehouseInventoryRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<List<WarehouseStockSummaryDto>>> Handle(
        GetWarehouseStockSummaryQuery request,
        CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return Result<List<WarehouseStockSummaryDto>>.Failure("Tenant not found");
        }

        var tenantId = _tenantContext.TenantId.Value;

        var inventories = await _warehouseInventoryRepository.GetAllWithDetailsForTenantAsync(tenantId, cancellationToken);

        var summary = inventories.Select(wi => new WarehouseStockSummaryDto
        {
            WarehouseCode = wi.Warehouse.Code,
            WarehouseName = wi.Warehouse.Name,
            ProductCode = wi.Product.Code,
            ProductName = wi.Product.Name,
            Brand = wi.Product.Brand,
            Category = wi.Product.Category,
            Quantity = wi.Quantity,
            ReservedQuantity = wi.ReservedQuantity,
            AvailableQuantity = wi.Quantity - wi.ReservedQuantity,
            LastMovementDate = wi.LastMovementDate.HasValue
                ? wi.LastMovementDate.Value.ToString("yyyy-MM-dd HH:mm:ss")
                : null
        }).ToList();

        return Result<List<WarehouseStockSummaryDto>>.Success(summary);
    }
}
