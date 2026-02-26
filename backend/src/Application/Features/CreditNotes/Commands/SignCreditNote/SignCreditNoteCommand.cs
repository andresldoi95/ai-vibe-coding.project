using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.CreditNotes.Commands.SignCreditNote;

public record SignCreditNoteCommand : IRequest<Result<string>>
{
    public Guid CreditNoteId { get; init; }
}
