using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.CreditNotes.Commands.GenerateCreditNoteXml;

public record GenerateCreditNoteXmlCommand : IRequest<Result<string>>
{
    public Guid CreditNoteId { get; init; }
}
