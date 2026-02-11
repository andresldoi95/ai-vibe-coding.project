using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Establishments.Queries.GetAllEstablishments;

/// <summary>
/// Query to get all establishments for the current tenant
/// </summary>
public record GetAllEstablishmentsQuery : IRequest<Result<List<EstablishmentDto>>>;
