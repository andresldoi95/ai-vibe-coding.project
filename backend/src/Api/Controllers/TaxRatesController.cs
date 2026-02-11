using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Features.TaxRates.Commands.CreateTaxRate;
using SaaS.Application.Features.TaxRates.Commands.DeleteTaxRate;
using SaaS.Application.Features.TaxRates.Commands.UpdateTaxRate;
using SaaS.Application.Features.TaxRates.Queries.GetAllTaxRates;
using SaaS.Application.Features.TaxRates.Queries.GetTaxRateById;

namespace SaaS.Api.Controllers;

[ApiController]
[Route("api/v1/taxrates")]
[Authorize]
public class TaxRatesController : BaseController
{
    public TaxRatesController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [Authorize(Policy = "tax-rates.read")]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllTaxRatesQuery();
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "tax-rates.read")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetTaxRateByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    [HttpPost]
    [Authorize(Policy = "tax-rates.create")]
    public async Task<IActionResult> Create([FromBody] CreateTaxRateCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            new { data = result.Value, success = true });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "tax-rates.update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaxRateCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(new { message = "ID mismatch", success = false });
        }

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "tax-rates.delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteTaxRateCommand(id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }
}
