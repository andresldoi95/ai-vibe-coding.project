using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.CreditNotes.Commands.UpdateCreditNote;

public record UpdateCreditNoteCommand : IRequest<Result<CreditNoteDto>>
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public DateTime? IssueDate { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public List<UpdateCreditNoteItemDto> Items { get; init; } = new();
}
