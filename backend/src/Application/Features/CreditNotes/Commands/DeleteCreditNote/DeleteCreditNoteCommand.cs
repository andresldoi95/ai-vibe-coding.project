using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.CreditNotes.Commands.DeleteCreditNote;

public record DeleteCreditNoteCommand(Guid Id) : IRequest<Result<bool>>;
