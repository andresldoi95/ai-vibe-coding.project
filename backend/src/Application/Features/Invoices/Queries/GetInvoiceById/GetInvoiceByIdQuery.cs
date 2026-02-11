using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Invoices.Queries.GetInvoiceById;

public record GetInvoiceByIdQuery(Guid Id) : IRequest<Result<InvoiceDto>>;
