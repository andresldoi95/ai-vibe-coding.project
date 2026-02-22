using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Invoices.Commands.SubmitToSri;

public record SubmitToSriCommand : IRequest<Result<string>>
{
    public Guid InvoiceId { get; init; }
}
