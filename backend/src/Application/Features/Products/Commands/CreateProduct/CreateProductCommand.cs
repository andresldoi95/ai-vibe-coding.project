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

    // Optional: Initial Inventory (Recommended approach)
    public int? InitialQuantity { get; init; }
    public Guid? InitialWarehouseId { get; init; }

    // Physical Properties
    public decimal? Weight { get; init; }
    public string? Dimensions { get; init; }

    // Status
    public bool IsActive { get; init; } = true;
}
