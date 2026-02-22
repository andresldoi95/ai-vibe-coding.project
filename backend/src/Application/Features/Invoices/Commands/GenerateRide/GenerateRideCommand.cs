using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Invoices.Commands.GenerateRide;

public record GenerateRideCommand : IRequest<Result<string>>
{
    public Guid InvoiceId { get; init; }
}
