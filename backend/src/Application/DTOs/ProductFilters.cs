namespace SaaS.Application.DTOs;

/// <summary>
/// Filter parameters for product search queries
/// </summary>
public class ProductFilters
{
    /// <summary>
    /// Filter by product name (contains search)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Filter by product code (contains search)
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Filter by SKU (exact match)
    /// </summary>
    public string? SKU { get; set; }

    /// <summary>
    /// Filter by category
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Filter by brand
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Filter by active status
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Filter by minimum price
    /// </summary>
    public decimal? MinPrice { get; set; }

    /// <summary>
    /// Filter by maximum price
    /// </summary>
    public decimal? MaxPrice { get; set; }

    /// <summary>
    /// Filter products with low stock (current stock <= minimum stock level)
    /// </summary>
    public bool? LowStockOnly { get; set; }
}
