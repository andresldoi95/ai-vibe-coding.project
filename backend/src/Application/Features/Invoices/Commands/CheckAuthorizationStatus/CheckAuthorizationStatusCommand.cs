using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs.Sri;

namespace SaaS.Application.Features.Invoices.Commands.CheckAuthorizationStatus;

public record CheckAuthorizationStatusCommand : IRequest<Result<SriAuthorizationResponse>>
{
    public Guid InvoiceId { get; init; }
}
