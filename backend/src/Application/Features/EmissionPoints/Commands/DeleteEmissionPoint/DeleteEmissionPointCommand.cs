using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.EmissionPoints.Commands.DeleteEmissionPoint;

/// <summary>
/// Command to delete an emission point
/// </summary>
public record DeleteEmissionPointCommand(Guid Id) : IRequest<Result<Unit>>;
