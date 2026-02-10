namespace SaaS.Application.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    
    // Basic Information
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SKU { get; set; } = string.Empty;

    // Category/Classification
    public string? Category { get; set; }
    public string? Brand { get; set; }

    // Pricing
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }

    // Inventory
    public int MinimumStockLevel { get; set; }
    public int CurrentStockLevel { get; set; }

    // Physical Properties
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }

    // Status
    public bool IsActive { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
