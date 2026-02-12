namespace SaaS.Application.DTOs;

public class WarehouseDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Address information
    public string StreetAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public Guid CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;

    // Contact information
    public string? Phone { get; set; }
    public string? Email { get; set; }

    // Additional properties
    public bool IsActive { get; set; }
    public decimal? SquareFootage { get; set; }
    public int? Capacity { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
