using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.CreditNotes.Queries.GetSriErrorLogsForCreditNote;

public record GetSriErrorLogsForCreditNoteQuery(Guid CreditNoteId) : IRequest<Result<List<SriErrorLogDto>>>;
