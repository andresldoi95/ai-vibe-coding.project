using MediatR;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Features.Countries.Queries.GetAllCountries;

namespace SaaS.Api.Controllers;

/// <summary>
/// Controller for managing countries
/// </summary>
[ApiController]
[Route("api/v1/countries")]
public class CountriesController : BaseController
{
    public CountriesController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get all active countries
    /// </summary>
    /// <returns>List of active countries</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool onlyActive = true)
    {
        var query = new GetAllCountriesQuery(onlyActive);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }
}
