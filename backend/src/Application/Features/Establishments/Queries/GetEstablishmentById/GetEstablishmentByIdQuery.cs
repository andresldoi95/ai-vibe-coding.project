using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Establishments.Queries.GetEstablishmentById;

/// <summary>
/// Query to get an establishment by ID
/// </summary>
public record GetEstablishmentByIdQuery(Guid Id) : IRequest<Result<EstablishmentDto>>;
