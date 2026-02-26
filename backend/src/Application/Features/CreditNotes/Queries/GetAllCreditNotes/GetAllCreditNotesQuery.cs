using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.CreditNotes.Queries.GetAllCreditNotes;

public record GetAllCreditNotesQuery : IRequest<Result<List<CreditNoteDto>>>
{
    public Guid? CustomerId { get; init; }
    public InvoiceStatus? Status { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
}
