using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.EmissionPoints.Commands.CreateEmissionPoint;

/// <summary>
/// Command to create a new emission point
/// </summary>
public record CreateEmissionPointCommand : IRequest<Result<EmissionPointDto>>
{
    public Guid EstablishmentId { get; init; }
    public string EmissionPointCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
}
