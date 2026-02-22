using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Features.Invoices.Commands.ChangeInvoiceStatus;
using SaaS.Application.Features.Invoices.Commands.CheckAuthorizationStatus;
using SaaS.Application.Features.Invoices.Commands.CreateInvoice;
using SaaS.Application.Features.Invoices.Commands.DeleteInvoice;
using SaaS.Application.Features.Invoices.Commands.GenerateInvoiceXml;
using SaaS.Application.Features.Invoices.Commands.GenerateRide;
using SaaS.Application.Features.Invoices.Commands.SignInvoice;
using SaaS.Application.Features.Invoices.Commands.SubmitToSri;
using SaaS.Application.Features.Invoices.Commands.UpdateInvoice;
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

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "invoices.update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInvoiceCommand command)
    {
        // Set ID from route parameter
        var commandWithId = command with { Id = id };

        var result = await _mediator.Send(commandWithId);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
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

    // SRI Electronic Invoicing Endpoints

    [HttpPost("{id:guid}/generate-xml")]
    [Authorize(Policy = "invoices.manage")]
    public async Task<IActionResult> GenerateXml(Guid id)
    {
        var command = new GenerateInvoiceXmlCommand { InvoiceId = id };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        // Fetch updated invoice
        var query = new GetInvoiceByIdQuery(id);
        var invoiceResult = await _mediator.Send(query);

        if (!invoiceResult.IsSuccess)
        {
            return Ok(new { data = new { xmlFilePath = result.Value }, message = "XML generated successfully", success = true });
        }

        return Ok(new { data = invoiceResult.Value, message = "XML generated successfully", success = true });
    }

    [HttpPost("{id:guid}/sign")]
    [Authorize(Policy = "invoices.manage")]
    public async Task<IActionResult> SignDocument(Guid id)
    {
        var command = new SignInvoiceCommand { InvoiceId = id };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        // Fetch updated invoice
        var query = new GetInvoiceByIdQuery(id);
        var invoiceResult = await _mediator.Send(query);

        if (!invoiceResult.IsSuccess)
        {
            return Ok(new { data = new { signedXmlFilePath = result.Value }, message = "Document signed successfully", success = true });
        }

        return Ok(new { data = invoiceResult.Value, message = "Document signed successfully", success = true });
    }

    [HttpPost("{id:guid}/submit-to-sri")]
    [Authorize(Policy = "invoices.manage")]
    public async Task<IActionResult> SubmitToSri(Guid id)
    {
        var command = new SubmitToSriCommand { InvoiceId = id };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        // Fetch updated invoice
        var query = new GetInvoiceByIdQuery(id);
        var invoiceResult = await _mediator.Send(query);

        if (!invoiceResult.IsSuccess)
        {
            return Ok(new { message = result.Value, success = true });
        }

        return Ok(new { data = invoiceResult.Value, message = result.Value, success = true });
    }

    [HttpGet("{id:guid}/check-authorization")]
    [Authorize(Policy = "invoices.read")]
    public async Task<IActionResult> CheckAuthorization(Guid id)
    {
        var command = new CheckAuthorizationStatusCommand { InvoiceId = id };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        // Fetch updated invoice
        var query = new GetInvoiceByIdQuery(id);
        var invoiceResult = await _mediator.Send(query);

        if (!invoiceResult.IsSuccess)
        {
            return Ok(new { data = result.Value, success = true });
        }

        return Ok(new { data = invoiceResult.Value, message = result.Value, success = true });
    }

    [HttpPost("{id:guid}/generate-ride")]
    [Authorize(Policy = "invoices.manage")]
    public async Task<IActionResult> GenerateRide(Guid id)
    {
        var command = new GenerateRideCommand { InvoiceId = id };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        // Fetch updated invoice
        var query = new GetInvoiceByIdQuery(id);
        var invoiceResult = await _mediator.Send(query);

        if (!invoiceResult.IsSuccess)
        {
            return Ok(new { data = new { rideFilePath = result.Value }, message = "RIDE PDF generated successfully", success = true });
        }

        return Ok(new { data = invoiceResult.Value, message = "RIDE PDF generated successfully", success = true });
    }

    [HttpGet("{id:guid}/download-xml")]
    [Authorize(Policy = "invoices.read")]
    public async Task<IActionResult> DownloadXml(Guid id)
    {
        var query = new GetInvoiceByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.Error, success = false });
        }

        var invoice = result.Value;
        if (string.IsNullOrEmpty(invoice.SignedXmlFilePath))
        {
            return NotFound(new { message = "Signed XML file not found", success = false });
        }

        if (!System.IO.File.Exists(invoice.SignedXmlFilePath))
        {
            return NotFound(new { message = "Signed XML file not found on disk", success = false });
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(invoice.SignedXmlFilePath);
        var fileName = $"{invoice.InvoiceNumber}_{invoice.AccessKey}.xml";

        return File(fileBytes, "application/xml", fileName);
    }

    [HttpGet("{id:guid}/download-ride")]
    [Authorize(Policy = "invoices.read")]
    public async Task<IActionResult> DownloadRide(Guid id)
    {
        var query = new GetInvoiceByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.Error, success = false });
        }

        var invoice = result.Value;
        if (string.IsNullOrEmpty(invoice.RideFilePath))
        {
            return NotFound(new { message = "RIDE PDF not found. Document may not be authorized yet.", success = false });
        }

        if (!System.IO.File.Exists(invoice.RideFilePath))
        {
            return NotFound(new { message = "RIDE PDF file not found on disk", success = false });
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(invoice.RideFilePath);
        var fileName = $"Factura_{invoice.InvoiceNumber}.pdf";

        return File(fileBytes, "application/pdf", fileName);
    }
}
