using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Customers.Queries.GetAllCustomers;

/// <summary>
/// Query to get all customers for the current tenant with optional filters
/// </summary>
public record GetAllCustomersQuery : IRequest<Result<List<CustomerDto>>>
{
    public CustomerFilters? Filters { get; init; }
}
