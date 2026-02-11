using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.EmissionPoints.Queries.GetAllEmissionPoints;

/// <summary>
/// Query to get all emission points for the current tenant
/// </summary>
public record GetAllEmissionPointsQuery : IRequest<Result<List<EmissionPointDto>>>
{
    public Guid? EstablishmentId { get; init; }
}
