using MediatR;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Countries.Queries.GetAllCountries;

/// <summary>
/// Handler for getting all countries
/// </summary>
public class GetAllCountriesQueryHandler : IRequestHandler<GetAllCountriesQuery, Result<List<CountryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllCountriesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<CountryDto>>> Handle(GetAllCountriesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var countries = request.OnlyActive
                ? await _unitOfWork.Countries.GetActiveCountriesAsync()
                : await _unitOfWork.Countries.GetAllAsync();

            var countryDtos = countries.Select(c => new CountryDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Alpha3Code = c.Alpha3Code,
                NumericCode = c.NumericCode,
                IsActive = c.IsActive
            }).ToList();

            return Result<List<CountryDto>>.Success(countryDtos);
        }
        catch (Exception ex)
        {
            return Result<List<CountryDto>>.Failure($"Error retrieving countries: {ex.Message}");
        }
    }
}
