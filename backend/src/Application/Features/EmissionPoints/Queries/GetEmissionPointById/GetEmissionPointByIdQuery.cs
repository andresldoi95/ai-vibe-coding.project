using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.EmissionPoints.Queries.GetEmissionPointById;

/// <summary>
/// Query to get an emission point by ID
/// </summary>
public record GetEmissionPointByIdQuery(Guid Id) : IRequest<Result<EmissionPointDto>>;
