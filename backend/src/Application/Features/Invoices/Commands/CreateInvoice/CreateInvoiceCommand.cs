using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Invoices.Commands.CreateInvoice;

public record CreateInvoiceCommand : IRequest<Result<InvoiceDto>>
{
    public Guid CustomerId { get; init; }
    public Guid? WarehouseId { get; init; }
    public DateTime? IssueDate { get; init; }
    public DateTime? DueDate { get; init; }
    public string? Notes { get; init; }
    public string? SriAuthorization { get; init; }
    public List<CreateInvoiceItemDto> Items { get; init; } = new();
}
