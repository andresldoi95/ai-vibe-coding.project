using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Features.InvoiceConfigurations.Commands.UpdateInvoiceConfiguration;
using SaaS.Application.Features.InvoiceConfigurations.Queries.GetInvoiceConfiguration;

namespace SaaS.Api.Controllers;

[ApiController]
[Route("api/v1/invoice-configurations")]
[Authorize]
public class InvoiceConfigurationsController : BaseController
{
    public InvoiceConfigurationsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [Authorize(Policy = "invoice-config.read")]
    public async Task<IActionResult> Get()
    {
        var query = new GetInvoiceConfigurationQuery();
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    [HttpPut]
    [Authorize(Policy = "invoice-config.update")]
    public async Task<IActionResult> Update([FromBody] UpdateInvoiceConfigurationCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }
}
