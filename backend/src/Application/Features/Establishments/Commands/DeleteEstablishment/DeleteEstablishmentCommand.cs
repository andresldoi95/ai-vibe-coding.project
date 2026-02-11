using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Establishments.Commands.DeleteEstablishment;

/// <summary>
/// Command to delete an establishment
/// </summary>
public record DeleteEstablishmentCommand(Guid Id) : IRequest<Result<Unit>>;
