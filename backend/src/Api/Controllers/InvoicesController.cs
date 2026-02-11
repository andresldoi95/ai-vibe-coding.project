using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Features.Invoices.Commands.ChangeInvoiceStatus;
using SaaS.Application.Features.Invoices.Commands.CreateInvoice;
using SaaS.Application.Features.Invoices.Commands.DeleteInvoice;
using SaaS.Application.Features.Invoices.Queries.GetAllInvoices;
using SaaS.Application.Features.Invoices.Queries.GetInvoiceById;
using SaaS.Domain.Enums;

namespace SaaS.Api.Controllers;

[ApiController]
[Route("api/v1/invoices")]
[Authorize]
public class InvoicesController : BaseController
{
    public InvoicesController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [Authorize(Policy = "invoices.read")]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? customerId,
        [FromQuery] InvoiceStatus? status,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        var query = new GetAllInvoicesQuery
        {
            CustomerId = customerId,
            Status = status,
            DateFrom = dateFrom,
            DateTo = dateTo
        };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "invoices.read")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetInvoiceByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    [HttpPost]
    [Authorize(Policy = "invoices.create")]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceCommand command)
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

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "invoices.update")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeInvoiceStatusCommand command)
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
    [Authorize(Policy = "invoices.delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteInvoiceCommand(id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }
}
