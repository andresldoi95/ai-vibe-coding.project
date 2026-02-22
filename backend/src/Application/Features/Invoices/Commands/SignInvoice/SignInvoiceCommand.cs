using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Invoices.Commands.SignInvoice;

public record SignInvoiceCommand : IRequest<Result<string>>
{
    public Guid InvoiceId { get; init; }
}
