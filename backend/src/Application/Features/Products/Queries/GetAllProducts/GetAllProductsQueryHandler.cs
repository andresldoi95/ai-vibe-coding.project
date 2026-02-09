using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Products.Queries.GetAllProducts;

/// <summary>
/// Handler for getting all products with optional filters
/// </summary>
public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, Result<List<ProductDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetAllProductsQueryHandler> _logger;

    public GetAllProductsQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetAllProductsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<List<ProductDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<List<ProductDto>>.Failure("Tenant context is required");
            }

            var products = await _unitOfWork.Products.GetAllByTenantAsync(
                _tenantContext.TenantId.Value,
                request.Filters,
                cancellationToken);

            // Get total stock levels from WarehouseInventory for all products
            var productIds = products.Select(p => p.Id).ToList();
            var inventoryDict = await _unitOfWork.WarehouseInventory
                .GetTotalStockByProductIdsAsync(productIds, _tenantContext.TenantId.Value, cancellationToken);

            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                TenantId = p.TenantId,
                Name = p.Name,
                Code = p.Code,
                Description = p.Description,
                SKU = p.SKU,
                Category = p.Category,
                Brand = p.Brand,
                UnitPrice = p.UnitPrice,
                CostPrice = p.CostPrice,
                MinimumStockLevel = p.MinimumStockLevel,
                CurrentStockLevel = inventoryDict.ContainsKey(p.Id) ? inventoryDict[p.Id] : 0,
                Weight = p.Weight,
                Dimensions = p.Dimensions,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();

            return Result<List<ProductDto>>.Success(productDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products");
            return Result<List<ProductDto>>.Failure("An error occurred while retrieving products");
        }
    }
}
