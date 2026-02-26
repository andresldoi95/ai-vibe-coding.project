using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.CreditNotes.Queries.GetCreditNoteById;

public record GetCreditNoteByIdQuery(Guid Id) : IRequest<Result<CreditNoteDto>>;
