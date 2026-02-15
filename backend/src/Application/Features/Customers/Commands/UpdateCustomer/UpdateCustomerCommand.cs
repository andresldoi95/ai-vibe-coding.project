using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Customers.Commands.UpdateCustomer;

/// <summary>
/// Command to update an existing customer
/// </summary>
public record UpdateCustomerCommand : IRequest<Result<CustomerDto>>
{
    public Guid Id { get; init; }

    // Basic Information
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? TaxId { get; init; }
    public string? ContactPerson { get; init; }

    // Billing Address
    public string? BillingStreet { get; init; }
    public string? BillingCity { get; init; }
    public string? BillingState { get; init; }
    public string? BillingPostalCode { get; init; }
    public Guid? BillingCountryId { get; init; }

    // Shipping Address
    public string? ShippingStreet { get; init; }
    public string? ShippingCity { get; init; }
    public string? ShippingState { get; init; }
    public string? ShippingPostalCode { get; init; }
    public Guid? ShippingCountryId { get; init; }

    // Additional Information
    public string? Notes { get; init; }
    public string? Website { get; init; }

    // Status
    public bool IsActive { get; init; }
}
