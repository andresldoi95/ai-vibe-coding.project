using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.Invoices.Queries.GetAllInvoices;

public record GetAllInvoicesQuery : IRequest<Result<List<InvoiceDto>>>
{
    public Guid? CustomerId { get; init; }
    public InvoiceStatus? Status { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
}
