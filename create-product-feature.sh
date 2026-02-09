#!/bin/bash

# Product Feature Implementation Script
# This script creates all necessary files for the Products feature

set -e

echo "Creating Product Feature Implementation..."

# Base directories
BACKEND_BASE="/home/runner/work/ai-vibe-coding.project/ai-vibe-coding.project/backend/src"
APP_DIR="$BACKEND_BASE/Application/Features/Products"
API_DIR="$BACKEND_BASE/Api/Controllers"

# Create directory structure
echo "Creating directory structure..."
mkdir -p "$APP_DIR/Commands/CreateProduct"
mkdir -p "$APP_DIR/Commands/UpdateProduct"
mkdir -p "$APP_DIR/Commands/DeleteProduct"
mkdir -p "$APP_DIR/Queries/GetAllProducts"
mkdir -p "$APP_DIR/Queries/GetProductById"

# ============================================================================
# CREATE PRODUCT COMMAND
# ============================================================================

echo "Creating CreateProductCommand..."
cat > "$APP_DIR/Commands/CreateProduct/CreateProductCommand.cs" << 'EOF'
using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Products.Commands.CreateProduct;

/// <summary>
/// Command to create a new product
/// </summary>
public record CreateProductCommand : IRequest<Result<ProductDto>>
{
    // Basic Information
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string SKU { get; init; } = string.Empty;

    // Category/Classification
    public string? Category { get; init; }
    public string? Brand { get; init; }

    // Pricing
    public decimal UnitPrice { get; init; }
    public decimal CostPrice { get; init; }

    // Inventory
    public int MinimumStockLevel { get; init; }
    public int? CurrentStockLevel { get; init; }

    // Physical Properties
    public decimal? Weight { get; init; }
    public string? Dimensions { get; init; }

    // Status
    public bool IsActive { get; init; } = true;
}
EOF

echo "Creating CreateProductCommandValidator..."
cat > "$APP_DIR/Commands/CreateProduct/CreateProductCommandValidator.cs" << 'EOF'
using FluentValidation;

namespace SaaS.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(256).WithMessage("Product name cannot exceed 256 characters");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Product code is required")
            .MaximumLength(50).WithMessage("Product code cannot exceed 50 characters")
            .Matches("^[A-Z0-9-]+$").WithMessage("Product code can only contain uppercase letters, numbers, and hyphens");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(100).WithMessage("SKU cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Category cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Category));

        RuleFor(x => x.Brand)
            .MaximumLength(100).WithMessage("Brand cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Brand));

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price must be zero or greater");

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Cost price must be zero or greater");

        RuleFor(x => x.MinimumStockLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum stock level must be zero or greater");

        RuleFor(x => x.CurrentStockLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Current stock level must be zero or greater")
            .When(x => x.CurrentStockLevel.HasValue);

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than 0")
            .When(x => x.Weight.HasValue);

        RuleFor(x => x.Dimensions)
            .MaximumLength(100).WithMessage("Dimensions cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Dimensions));
    }
}
EOF

echo "Creating CreateProductCommandHandler..."
cat > "$APP_DIR/Commands/CreateProduct/CreateProductCommandHandler.cs" << 'EOF'
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
EOF

# ============================================================================
# UPDATE PRODUCT COMMAND
# ============================================================================

echo "Creating UpdateProductCommand..."
cat > "$APP_DIR/Commands/UpdateProduct/UpdateProductCommand.cs" << 'EOF'
using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Products.Commands.UpdateProduct;

/// <summary>
/// Command to update an existing product
/// </summary>
public record UpdateProductCommand : IRequest<Result<ProductDto>>
{
    public Guid Id { get; init; }
    
    // Basic Information
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string SKU { get; init; } = string.Empty;

    // Category/Classification
    public string? Category { get; init; }
    public string? Brand { get; init; }

    // Pricing
    public decimal UnitPrice { get; init; }
    public decimal CostPrice { get; init; }

    // Inventory
    public int MinimumStockLevel { get; init; }
    public int? CurrentStockLevel { get; init; }

    // Physical Properties
    public decimal? Weight { get; init; }
    public string? Dimensions { get; init; }

    // Status
    public bool IsActive { get; init; }
}
EOF

echo "Creating UpdateProductCommandValidator..."
cat > "$APP_DIR/Commands/UpdateProduct/UpdateProductCommandValidator.cs" << 'EOF'
using FluentValidation;

namespace SaaS.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(256).WithMessage("Product name cannot exceed 256 characters");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Product code is required")
            .MaximumLength(50).WithMessage("Product code cannot exceed 50 characters")
            .Matches("^[A-Z0-9-]+$").WithMessage("Product code can only contain uppercase letters, numbers, and hyphens");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(100).WithMessage("SKU cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Category cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Category));

        RuleFor(x => x.Brand)
            .MaximumLength(100).WithMessage("Brand cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Brand));

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price must be zero or greater");

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Cost price must be zero or greater");

        RuleFor(x => x.MinimumStockLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum stock level must be zero or greater");

        RuleFor(x => x.CurrentStockLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Current stock level must be zero or greater")
            .When(x => x.CurrentStockLevel.HasValue);

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than 0")
            .When(x => x.Weight.HasValue);

        RuleFor(x => x.Dimensions)
            .MaximumLength(100).WithMessage("Dimensions cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Dimensions));
    }
}
EOF

echo "Creating UpdateProductCommandHandler..."
cat > "$APP_DIR/Commands/UpdateProduct/UpdateProductCommandHandler.cs" << 'EOF'
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

            _unitOfWork.Products.Update(product);
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
EOF

# ============================================================================
# DELETE PRODUCT COMMAND
# ============================================================================

echo "Creating DeleteProductCommand..."
cat > "$APP_DIR/Commands/DeleteProduct/DeleteProductCommand.cs" << 'EOF'
using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Command to delete a product (soft delete)
/// </summary>
public record DeleteProductCommand : IRequest<Result<bool>>
{
    public Guid Id { get; init; }
}
EOF

echo "Creating DeleteProductCommandHandler..."
cat > "$APP_DIR/Commands/DeleteProduct/DeleteProductCommandHandler.cs" << 'EOF'
using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Handler for deleting a product (soft delete)
/// </summary>
public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<DeleteProductCommandHandler> _logger;

    public DeleteProductCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<DeleteProductCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<bool>.Failure("Tenant context is required");
            }

            var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);

            if (product == null)
            {
                return Result<bool>.Failure("Product not found");
            }

            // Verify tenant ownership
            if (product.TenantId != _tenantContext.TenantId.Value)
            {
                return Result<bool>.Failure("Product not found");
            }

            // Soft delete
            product.IsDeleted = true;
            product.DeletedAt = DateTime.UtcNow;

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Product {Code} deleted successfully for tenant {TenantId}",
                product.Code,
                product.TenantId);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product");
            return Result<bool>.Failure("An error occurred while deleting the product");
        }
    }
}
EOF

# ============================================================================
# GET ALL PRODUCTS QUERY
# ============================================================================

echo "Creating GetAllProductsQuery..."
cat > "$APP_DIR/Queries/GetAllProducts/GetAllProductsQuery.cs" << 'EOF'
using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Products.Queries.GetAllProducts;

/// <summary>
/// Query to get all products for the current tenant with optional filters
/// </summary>
public record GetAllProductsQuery : IRequest<Result<List<ProductDto>>>
{
    public ProductFilters? Filters { get; init; }
}
EOF

echo "Creating GetAllProductsQueryHandler..."
cat > "$APP_DIR/Queries/GetAllProducts/GetAllProductsQueryHandler.cs" << 'EOF'
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
                CurrentStockLevel = p.CurrentStockLevel,
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
EOF

# ============================================================================
# GET PRODUCT BY ID QUERY
# ============================================================================

echo "Creating GetProductByIdQuery..."
cat > "$APP_DIR/Queries/GetProductById/GetProductByIdQuery.cs" << 'EOF'
using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Products.Queries.GetProductById;

/// <summary>
/// Query to get a product by ID
/// </summary>
public record GetProductByIdQuery : IRequest<Result<ProductDto>>
{
    public Guid Id { get; init; }
}
EOF

echo "Creating GetProductByIdQueryHandler..."
cat > "$APP_DIR/Queries/GetProductById/GetProductByIdQueryHandler.cs" << 'EOF'
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
            _logger.LogError(ex, "Error getting product by ID");
            return Result<ProductDto>.Failure("An error occurred while retrieving the product");
        }
    }
}
EOF

# ============================================================================
# PRODUCTS CONTROLLER
# ============================================================================

echo "Creating ProductsController..."
cat > "$API_DIR/ProductsController.cs" << 'EOF'
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.DTOs;
using SaaS.Application.Features.Products.Commands.CreateProduct;
using SaaS.Application.Features.Products.Commands.DeleteProduct;
using SaaS.Application.Features.Products.Commands.UpdateProduct;
using SaaS.Application.Features.Products.Queries.GetAllProducts;
using SaaS.Application.Features.Products.Queries.GetProductById;

namespace SaaS.Api.Controllers;

[ApiController]
[Route("api/v1/products")]
[Authorize]
public class ProductsController : BaseController
{
    public ProductsController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get all products for the current tenant with optional filters
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "products.read")]
    public async Task<IActionResult> GetAll([FromQuery] ProductFilters? filters)
    {
        var query = new GetAllProductsQuery { Filters = filters };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    /// <summary>
    /// Get a product by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "products.read")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetProductByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "products.create")]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess || result.Value == null)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value.Id },
            new { data = result.Value, success = true });
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "products.update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(new { message = "ID mismatch", success = false });
        }

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    /// <summary>
    /// Delete a product (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "products.delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteProductCommand { Id = id };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { message = "Product deleted successfully", success = true });
    }
}
EOF

echo ""
echo "✅ Product Feature files created successfully!"
echo ""
echo "Next steps:"
echo "1. Update ApplicationDbContext to add Products DbSet"
echo "2. Update IUnitOfWork and UnitOfWork to add Products property"
echo "3. Register ProductRepository in Program.cs"
echo "4. Run migration: dotnet ef migrations add AddProductEntity -p backend/src/Infrastructure -s backend/src/Api"
echo "5. Build and restart backend"
