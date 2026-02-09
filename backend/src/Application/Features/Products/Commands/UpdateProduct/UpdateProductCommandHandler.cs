using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Products.Commands.UpdateProduct;

/// <summary>
/// Handler for updating an existing product
/// </summary>
public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<UpdateProductCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<ProductDto>.Failure("Tenant context is required");
            }

            // Get existing product
            var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);

            if (product == null)
            {
                return Result<ProductDto>.Failure("Product not found");
            }

            // Verify tenant ownership
            if (product.TenantId != _tenantContext.TenantId.Value)
            {
                return Result<ProductDto>.Failure("Product not found");
            }

            // Check if new code conflicts with another product
            if (product.Code != request.Code)
            {
                var existingProductByCode = await _unitOfWork.Products.GetByCodeAsync(
                    request.Code,
                    _tenantContext.TenantId.Value,
                    cancellationToken);

                if (existingProductByCode != null && existingProductByCode.Id != request.Id)
                {
                    return Result<ProductDto>.Failure($"Product with code '{request.Code}' already exists");
                }
            }

            // Check if new SKU conflicts with another product
            if (product.SKU != request.SKU)
            {
                var existingProductBySKU = await _unitOfWork.Products.GetBySKUAsync(
                    request.SKU,
                    _tenantContext.TenantId.Value,
                    cancellationToken);

                if (existingProductBySKU != null && existingProductBySKU.Id != request.Id)
                {
                    return Result<ProductDto>.Failure($"Product with SKU '{request.SKU}' already exists");
                }
            }

            // Update product properties
            product.Name = request.Name;
            product.Code = request.Code;
            product.Description = request.Description;
            product.SKU = request.SKU;
            product.Category = request.Category;
            product.Brand = request.Brand;
            product.UnitPrice = request.UnitPrice;
            product.CostPrice = request.CostPrice;
            product.MinimumStockLevel = request.MinimumStockLevel;
            product.CurrentStockLevel = request.CurrentStockLevel;
            product.Weight = request.Weight;
            product.Dimensions = request.Dimensions;
            product.IsActive = request.IsActive;

            await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Product {Code} updated successfully for tenant {TenantId}",
                product.Code,
                product.TenantId);

            // Map to DTO
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
                CurrentStockLevel = product.CurrentStockLevel,
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
            _logger.LogError(ex, "Error updating product");
            return Result<ProductDto>.Failure("An error occurred while updating the product");
        }
    }
}
