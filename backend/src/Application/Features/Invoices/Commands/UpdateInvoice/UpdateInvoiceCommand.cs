using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Invoices.Commands.UpdateInvoice;

public record UpdateInvoiceCommand : IRequest<Result<InvoiceDto>>
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public Guid? WarehouseId { get; init; }
    public DateTime? IssueDate { get; init; }
    public string? Notes { get; init; }
    public List<UpdateInvoiceItemDto> Items { get; init; } = new();
}
