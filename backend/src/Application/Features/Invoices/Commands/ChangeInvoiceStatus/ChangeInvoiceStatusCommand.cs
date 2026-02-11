using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.Invoices.Commands.ChangeInvoiceStatus;

public record ChangeInvoiceStatusCommand : IRequest<Result<InvoiceDto>>
{
    public Guid Id { get; init; }
    public InvoiceStatus NewStatus { get; init; }
}
