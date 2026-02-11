using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.SriConfiguration.Queries.GetSriConfiguration;

/// <summary>
/// Query to get SRI configuration for the current tenant
/// </summary>
public record GetSriConfigurationQuery : IRequest<Result<SriConfigurationDto>>;
