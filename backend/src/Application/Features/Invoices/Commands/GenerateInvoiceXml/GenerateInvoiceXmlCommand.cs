using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Invoices.Commands.GenerateInvoiceXml;

public record GenerateInvoiceXmlCommand : IRequest<Result<string>>
{
    public Guid InvoiceId { get; init; }
}
