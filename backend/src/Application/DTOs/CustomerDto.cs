namespace SaaS.Application.DTOs;

public class CustomerDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    
    // Basic Information
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? TaxId { get; set; }
    public string? ContactPerson { get; set; }

    // Billing Address
    public string? BillingStreet { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingPostalCode { get; set; }
    public string? BillingCountry { get; set; }

    // Shipping Address
    public string? ShippingStreet { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingPostalCode { get; set; }
    public string? ShippingCountry { get; set; }

    // Additional Information
    public string? Notes { get; set; }
    public string? Website { get; set; }

    // Status
    public bool IsActive { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
