using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Invoices.Commands.DeleteInvoice;

public record DeleteInvoiceCommand(Guid Id) : IRequest<Result<bool>>;
