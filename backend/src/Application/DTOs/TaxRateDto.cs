namespace SaaS.Application.DTOs;

public class TaxRateDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public string? Country { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
