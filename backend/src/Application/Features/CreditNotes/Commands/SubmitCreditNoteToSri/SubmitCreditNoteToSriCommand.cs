using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.CreditNotes.Commands.SubmitCreditNoteToSri;

public record SubmitCreditNoteToSriCommand : IRequest<Result<string>>
{
    public Guid CreditNoteId { get; init; }
}
