namespace SaaS.Application.DTOs;

/// <summary>
/// Filter parameters for customer search queries
/// </summary>
public class CustomerFilters
{
    /// <summary>
    /// Search term to filter across name, email, phone, and contact person
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Filter by customer name (contains search)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Filter by email (contains search)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Filter by phone (exact match)
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Filter by tax ID
    /// </summary>
    public string? TaxId { get; set; }

    /// <summary>
    /// Filter by billing city
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Filter by billing country
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Filter by active status
    /// </summary>
    public bool? IsActive { get; set; }
}
