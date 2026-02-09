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
