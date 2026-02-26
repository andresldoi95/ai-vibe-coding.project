using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.CreditNotes.Commands.CreateCreditNote;

public record CreateCreditNoteCommand : IRequest<Result<CreditNoteDto>>
{
    public Guid CustomerId { get; init; }
    public Guid OriginalInvoiceId { get; init; }
    public Guid EmissionPointId { get; init; }
    public DateTime? IssueDate { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public List<CreateCreditNoteItemDto> Items { get; init; } = new();
}
