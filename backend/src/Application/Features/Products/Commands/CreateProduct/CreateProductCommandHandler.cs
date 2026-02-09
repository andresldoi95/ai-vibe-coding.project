using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Entities;

namespace SaaS.Application.Features.Products.Commands.CreateProduct;

/// <summary>
/// Handler for creating a new product
/// </summary>
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<CreateProductCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<ProductDto>.Failure("Tenant context is required");
            }

            // Check if product code already exists for this tenant
            var existingProductByCode = await _unitOfWork.Products.GetByCodeAsync(
                request.Code,
                _tenantContext.TenantId.Value,
                cancellationToken);

            if (existingProductByCode != null)
            {
                return Result<ProductDto>.Failure($"Product with code '{request.Code}' already exists");
            }

            // Check if SKU already exists for this tenant
            var existingProductBySKU = await _unitOfWork.Products.GetBySKUAsync(
                request.SKU,
                _tenantContext.TenantId.Value,
                cancellationToken);

            if (existingProductBySKU != null)
            {
                return Result<ProductDto>.Failure($"Product with SKU '{request.SKU}' already exists");
            }

            // Create product entity
            var product = new Product
            {
                TenantId = _tenantContext.TenantId.Value,
                Name = request.Name,
                Code = request.Code,
                Description = request.Description,
                SKU = request.SKU,
                Category = request.Category,
                Brand = request.Brand,
                UnitPrice = request.UnitPrice,
                CostPrice = request.CostPrice,
                MinimumStockLevel = request.MinimumStockLevel,
                CurrentStockLevel = request.CurrentStockLevel,
                Weight = request.Weight,
                Dimensions = request.Dimensions,
                IsActive = request.IsActive
            };

            await _unitOfWork.Products.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Product {Code} created successfully for tenant {TenantId}",
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
            _logger.LogError(ex, "Error creating product");
            return Result<ProductDto>.Failure("An error occurred while creating the product");
        }
    }
}
