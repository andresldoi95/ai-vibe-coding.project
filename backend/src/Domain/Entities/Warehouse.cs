using SaaS.Domain.Common;

namespace SaaS.Domain.Entities;

/// <summary>
/// Warehouse entity representing physical storage locations for inventory management
/// </summary>
public class Warehouse : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Address information
    public string StreetAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public Guid CountryId { get; set; }
    public Country Country { get; set; } = null!;

    // Contact information
    public string? Phone { get; set; }
    public string? Email { get; set; }

    // Additional properties
    public bool IsActive { get; set; } = true;
    public decimal? SquareFootage { get; set; }
    public int? Capacity { get; set; }
}
