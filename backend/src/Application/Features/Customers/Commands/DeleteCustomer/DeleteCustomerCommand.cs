using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Customers.Commands.DeleteCustomer;

/// <summary>
/// Command to delete a customer (soft delete)
/// </summary>
public record DeleteCustomerCommand : IRequest<Result<bool>>
{
    public Guid Id { get; init; }
}
