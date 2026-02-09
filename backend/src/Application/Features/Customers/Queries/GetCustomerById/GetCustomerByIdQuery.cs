using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Customers.Queries.GetCustomerById;

/// <summary>
/// Query to get a customer by ID
/// </summary>
public record GetCustomerByIdQuery : IRequest<Result<CustomerDto>>
{
    public Guid Id { get; init; }
}
