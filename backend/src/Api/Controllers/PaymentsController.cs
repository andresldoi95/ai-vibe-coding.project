using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Features.Payments.Commands.CreatePayment;
using SaaS.Application.Features.Payments.Commands.VoidPayment;
using SaaS.Application.Features.Payments.Queries.GetAllPayments;
using SaaS.Application.Features.Payments.Queries.GetPaymentById;
using SaaS.Application.Features.Payments.Queries.GetPaymentsByInvoiceId;

namespace SaaS.Api.Controllers;

[ApiController]
[Route("api/v1/payments")]
[Authorize]
public class PaymentsController : BaseController
{
    public PaymentsController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get all payments for the current tenant
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "payments.read")]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllPaymentsQuery();
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    /// <summary>
    /// Get a payment by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "payments.read")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetPaymentByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    /// <summary>
    /// Get all payments for a specific invoice
    /// </summary>
    [HttpGet("invoice/{invoiceId:guid}")]
    [Authorize(Policy = "payments.read")]
    public async Task<IActionResult> GetByInvoiceId(Guid invoiceId)
    {
        var query = new GetPaymentsByInvoiceIdQuery(invoiceId);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    /// <summary>
    /// Create a new payment
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "payments.create")]
    public async Task<IActionResult> Create([FromBody] CreatePaymentCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess || result.Value == null)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value.Id },
            new { data = result.Value, success = true });
    }

    /// <summary>
    /// Void an existing payment
    /// </summary>
    [HttpPut("{id:guid}/void")]
    [Authorize(Policy = "payments.void")]
    public async Task<IActionResult> Void(Guid id, [FromBody] VoidPaymentRequest request)
    {
        var command = new VoidPaymentCommand { Id = id, Reason = request.Reason };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }
}

/// <summary>
/// Request model for voiding a payment
/// </summary>
public class VoidPaymentRequest
{
    public string? Reason { get; set; }
}
