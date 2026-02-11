using SaaS.Domain.Common;
using SaaS.Domain.Enums;

namespace SaaS.Domain.Entities;

/// <summary>
/// Customer entity representing clients in the billing system
/// </summary>
public class Customer : TenantEntity
{
    // Basic Information
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }

    // SRI Ecuador Identification
    public IdentificationType IdentificationType { get; set; } = IdentificationType.Cedula;
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
    public bool IsActive { get; set; } = true;
}
