using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Products.Queries.GetProductById;

/// <summary>
/// Handler for getting a product by ID
/// </summary>
public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetProductByIdQueryHandler> _logger;

    public GetProductByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetProductByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<ProductDto>.Failure("Tenant context is required");
            }

            var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);

            if (product == null || product.TenantId != _tenantContext.TenantId.Value)
            {
                return Result<ProductDto>.Failure("Product not found");
            }

            // Calculate total stock from WarehouseInventory
            var totalStock = await _unitOfWork.WarehouseInventory
                .GetTotalStockByProductIdAsync(product.Id, _tenantContext.TenantId.Value, cancellationToken);

            var productDto = new ProductDto
            {
                Id = product.Id,
                TenantId = product.TenantId,
                Name = product.Name,
                Code = product.Code,
                Description = product.Description,
                SKU = product.SKU,
                Category = product.Category,
                Brand = product.Brand,
                UnitPrice = product.UnitPrice,
                CostPrice = product.CostPrice,
                MinimumStockLevel = product.MinimumStockLevel,
                CurrentStockLevel = totalStock,
                Weight = product.Weight,
                Dimensions = product.Dimensions,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };

            return Result<ProductDto>.Success(productDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product by ID");
            return Result<ProductDto>.Failure("An error occurred while retrieving the product");
        }
    }
}
