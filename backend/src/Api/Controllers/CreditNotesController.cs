using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Features.CreditNotes.Commands.CheckCreditNoteAuthorizationStatus;
using SaaS.Application.Features.CreditNotes.Commands.CreateCreditNote;
using SaaS.Application.Features.CreditNotes.Commands.DeleteCreditNote;
using SaaS.Application.Features.CreditNotes.Commands.GenerateCreditNoteRide;
using SaaS.Application.Features.CreditNotes.Commands.GenerateCreditNoteXml;
using SaaS.Application.Features.CreditNotes.Commands.SignCreditNote;
using SaaS.Application.Features.CreditNotes.Commands.SubmitCreditNoteToSri;
using SaaS.Application.Features.CreditNotes.Commands.UpdateCreditNote;
using SaaS.Application.Features.CreditNotes.Queries.GetAllCreditNotes;
using SaaS.Application.Features.CreditNotes.Queries.GetCreditNoteById;
using SaaS.Application.Features.CreditNotes.Queries.GetSriErrorLogsForCreditNote;
using SaaS.Domain.Enums;

namespace SaaS.Api.Controllers;

[ApiController]
[Route("api/v1/credit-notes")]
[Authorize]
public class CreditNotesController : BaseController
{
    public CreditNotesController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [Authorize(Policy = "credit-notes.read")]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? customerId,
        [FromQuery] InvoiceStatus? status,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        var query = new GetAllCreditNotesQuery
        {
            CustomerId = customerId,
            Status = status,
            DateFrom = dateFrom,
            DateTo = dateTo
        };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error, success = false });

        return Ok(new { data = result.Value, success = true });
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "credit-notes.read")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetCreditNoteByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(new { message = result.Error, success = false });

        return Ok(new { data = result.Value, success = true });
    }

    [HttpPost]
    [Authorize(Policy = "credit-notes.create")]
    public async Task<IActionResult> Create([FromBody] CreateCreditNoteCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error, success = false });

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            new { data = result.Value, success = true });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "credit-notes.update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCreditNoteCommand command)
    {
        var commandWithId = command with { Id = id };
        var result = await _mediator.Send(commandWithId);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error, success = false });

        return Ok(new { data = result.Value, success = true });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "credit-notes.delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteCreditNoteCommand(id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error, success = false });

        return Ok(new { data = result.Value, success = true });
    }

    // ── SRI Electronic Document Endpoints ────────────────────────────────────

    [HttpPost("{id:guid}/generate-xml")]
    [Authorize(Policy = "credit-notes.manage")]
    public async Task<IActionResult> GenerateXml(Guid id)
    {
        var command = new GenerateCreditNoteXmlCommand { CreditNoteId = id };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error, success = false });

        var query = new GetCreditNoteByIdQuery(id);
        var creditNoteResult = await _mediator.Send(query);

        return creditNoteResult.IsSuccess
            ? Ok(new { data = creditNoteResult.Value, message = "XML generated successfully", success = true })
            : Ok(new { data = new { xmlFilePath = result.Value }, message = "XML generated successfully", success = true });
    }

    [HttpPost("{id:guid}/sign")]
    [Authorize(Policy = "credit-notes.manage")]
    public async Task<IActionResult> SignDocument(Guid id)
    {
        var command = new SignCreditNoteCommand { CreditNoteId = id };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error, success = false });

        var query = new GetCreditNoteByIdQuery(id);
        var creditNoteResult = await _mediator.Send(query);

        return creditNoteResult.IsSuccess
            ? Ok(new { data = creditNoteResult.Value, message = "Document signed successfully", success = true })
            : Ok(new { data = new { signedXmlFilePath = result.Value }, message = "Document signed successfully", success = true });
    }

    [HttpPost("{id:guid}/submit-sri")]
    [Authorize(Policy = "credit-notes.manage")]
    public async Task<IActionResult> SubmitToSri(Guid id)
    {
        var command = new SubmitCreditNoteToSriCommand { CreditNoteId = id };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error, success = false });

        var query = new GetCreditNoteByIdQuery(id);
        var creditNoteResult = await _mediator.Send(query);

        return creditNoteResult.IsSuccess
            ? Ok(new { data = creditNoteResult.Value, message = result.Value, success = true })
            : Ok(new { message = result.Value, success = true });
    }

    [HttpPost("{id:guid}/check-authorization")]
    [Authorize(Policy = "credit-notes.manage")]
    public async Task<IActionResult> CheckAuthorization(Guid id)
    {
        var command = new CheckCreditNoteAuthorizationStatusCommand { CreditNoteId = id };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error, success = false });

        var query = new GetCreditNoteByIdQuery(id);
        var creditNoteResult = await _mediator.Send(query);

        return creditNoteResult.IsSuccess
            ? Ok(new { data = creditNoteResult.Value, message = result.Value, success = true })
            : Ok(new { data = result.Value, success = true });
    }

    [HttpPost("{id:guid}/generate-ride")]
    [Authorize(Policy = "credit-notes.manage")]
    public async Task<IActionResult> GenerateRide(Guid id)
    {
        var command = new GenerateCreditNoteRideCommand { CreditNoteId = id };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error, success = false });

        var query = new GetCreditNoteByIdQuery(id);
        var creditNoteResult = await _mediator.Send(query);

        return creditNoteResult.IsSuccess
            ? Ok(new { data = creditNoteResult.Value, message = "RIDE PDF generated successfully", success = true })
            : Ok(new { data = new { rideFilePath = result.Value }, message = "RIDE PDF generated successfully", success = true });
    }

    [HttpGet("{id:guid}/sri-errors")]
    [Authorize(Policy = "credit-notes.read")]
    public async Task<IActionResult> GetSriErrors(Guid id)
    {
        var query = new GetSriErrorLogsForCreditNoteQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(new { message = result.Error, success = false });

        return Ok(new { data = result.Value, success = true });
    }

    [HttpGet("{id:guid}/download-xml")]
    [Authorize(Policy = "credit-notes.read")]
    public async Task<IActionResult> DownloadXml(Guid id)
    {
        var query = new GetCreditNoteByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(new { message = result.Error, success = false });

        var creditNote = result.Value;

        if (string.IsNullOrEmpty(creditNote.SignedXmlFilePath))
            return NotFound(new { message = "Signed XML file not found", success = false });

        if (!System.IO.File.Exists(creditNote.SignedXmlFilePath))
            return NotFound(new { message = "Signed XML file not found on disk", success = false });

        var fileBytes = await System.IO.File.ReadAllBytesAsync(creditNote.SignedXmlFilePath);
        var fileName = $"{creditNote.CreditNoteNumber}_{creditNote.AccessKey}.xml";

        return File(fileBytes, "application/xml", fileName);
    }

    [HttpGet("{id:guid}/download-ride")]
    [Authorize(Policy = "credit-notes.read")]
    public async Task<IActionResult> DownloadRide(Guid id)
    {
        var query = new GetCreditNoteByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(new { message = result.Error, success = false });

        var creditNote = result.Value;

        if (string.IsNullOrEmpty(creditNote.RideFilePath))
            return NotFound(new { message = "RIDE PDF not found. Document may not be authorized yet.", success = false });

        if (!System.IO.File.Exists(creditNote.RideFilePath))
            return NotFound(new { message = "RIDE PDF file not found on disk", success = false });

        var fileBytes = await System.IO.File.ReadAllBytesAsync(creditNote.RideFilePath);
        var fileName = $"NotaCredito_{creditNote.CreditNoteNumber}.pdf";

        return File(fileBytes, "application/pdf", fileName);
    }
}
