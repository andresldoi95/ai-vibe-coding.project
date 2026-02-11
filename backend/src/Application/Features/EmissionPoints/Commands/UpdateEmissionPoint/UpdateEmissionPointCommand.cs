using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.EmissionPoints.Commands.UpdateEmissionPoint;

/// <summary>
/// Command to update an existing emission point
/// </summary>
public record UpdateEmissionPointCommand : IRequest<Result<EmissionPointDto>>
{
    public Guid Id { get; init; }
    public Guid EstablishmentId { get; init; }
    public string EmissionPointCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
}
