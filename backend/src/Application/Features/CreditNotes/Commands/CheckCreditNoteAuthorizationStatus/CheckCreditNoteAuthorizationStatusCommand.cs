using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs.Sri;

namespace SaaS.Application.Features.CreditNotes.Commands.CheckCreditNoteAuthorizationStatus;

public record CheckCreditNoteAuthorizationStatusCommand : IRequest<Result<SriAuthorizationResponse>>
{
    public Guid CreditNoteId { get; init; }
}
