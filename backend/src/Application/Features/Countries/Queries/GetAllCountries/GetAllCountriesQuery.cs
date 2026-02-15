using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Countries.Queries.GetAllCountries;

/// <summary>
/// Query to get all countries or only active countries
/// </summary>
public record GetAllCountriesQuery(bool OnlyActive = true) : IRequest<Result<List<CountryDto>>>;
