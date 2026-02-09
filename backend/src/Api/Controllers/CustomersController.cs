using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.DTOs;
using SaaS.Application.Features.Customers.Commands.CreateCustomer;
using SaaS.Application.Features.Customers.Commands.DeleteCustomer;
using SaaS.Application.Features.Customers.Commands.UpdateCustomer;
using SaaS.Application.Features.Customers.Queries.GetAllCustomers;
using SaaS.Application.Features.Customers.Queries.GetCustomerById;

namespace SaaS.Api.Controllers;

[ApiController]
[Route("api/v1/customers")]
[Authorize]
public class CustomersController : BaseController
{
    public CustomersController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get all customers for the current tenant with optional filters
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "customers.read")]
    public async Task<IActionResult> GetAll([FromQuery] CustomerFilters? filters)
    {
        var query = new GetAllCustomersQuery { Filters = filters };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    /// <summary>
    /// Get a customer by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "customers.read")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetCustomerByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "customers.create")]
    public async Task<IActionResult> Create([FromBody] CreateCustomerCommand command)
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
    /// Update an existing customer
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "customers.update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerCommand command)
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

    /// <summary>
    /// Delete a customer (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "customers.delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteCustomerCommand { Id = id };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { message = "Customer deleted successfully", success = true });
    }
}
