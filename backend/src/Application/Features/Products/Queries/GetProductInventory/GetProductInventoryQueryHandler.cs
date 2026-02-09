using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Products.Queries.GetProductInventory;

/// <summary>
/// Handler for getting warehouse inventory for a specific product
/// </summary>
public class GetProductInventoryQueryHandler : IRequestHandler<GetProductInventoryQuery, Result<List<WarehouseInventoryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetProductInventoryQueryHandler> _logger;

    public GetProductInventoryQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetProductInventoryQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<List<WarehouseInventoryDto>>> Handle(GetProductInventoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<List<WarehouseInventoryDto>>.Failure("Tenant context is required");
            }

            var tenantId = _tenantContext.TenantId.Value;

            // Verify product exists and belongs to tenant
            var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null || product.TenantId != tenantId)
            {
                return Result<List<WarehouseInventoryDto>>.Failure("Product not found");
            }

            // Get inventory for this product across all warehouses
            var inventoryRecords = await _unitOfWork.WarehouseInventory.GetByProductIdAsync(
                request.ProductId,
                tenantId,
                cancellationToken);

            // Map to DTOs
            var dtos = inventoryRecords.Select(inv => new WarehouseInventoryDto
            {
                Id = inv.Id,
                TenantId = inv.TenantId,
                ProductId = inv.ProductId,
                ProductName = inv.Product.Name,
                ProductCode = inv.Product.Code,
                WarehouseId = inv.WarehouseId,
                WarehouseName = inv.Warehouse.Name,
                WarehouseCode = inv.Warehouse.Code,
                Quantity = inv.Quantity,
                ReservedQuantity = inv.ReservedQuantity,
                AvailableQuantity = inv.AvailableQuantity,
                LastMovementDate = inv.LastMovementDate,
                CreatedAt = inv.CreatedAt,
                UpdatedAt = inv.UpdatedAt
            }).ToList();

            return Result<List<WarehouseInventoryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product inventory for product {ProductId}", request.ProductId);
            return Result<List<WarehouseInventoryDto>>.Failure("An error occurred while retrieving product inventory");
        }
    }
}
