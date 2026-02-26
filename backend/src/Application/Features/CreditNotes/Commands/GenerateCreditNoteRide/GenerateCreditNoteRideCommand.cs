using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.CreditNotes.Commands.GenerateCreditNoteRide;

public record GenerateCreditNoteRideCommand : IRequest<Result<string>>
{
    public Guid CreditNoteId { get; init; }
}
