using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.DTOs;
using SaaS.Application.Features.Products.Commands.CreateProduct;
using SaaS.Application.Features.Products.Commands.DeleteProduct;
using SaaS.Application.Features.Products.Commands.UpdateProduct;
using SaaS.Application.Features.Products.Queries.GetAllProducts;
using SaaS.Application.Features.Products.Queries.GetProductById;
using SaaS.Application.Features.Products.Queries.GetProductInventory;

namespace SaaS.Api.Controllers;

[ApiController]
[Route("api/v1/products")]
[Authorize]
public class ProductsController : BaseController
{
    public ProductsController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get all products for the current tenant with optional filters
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "products.read")]
    public async Task<IActionResult> GetAll([FromQuery] ProductFilters? filters)
    {
        var query = new GetAllProductsQuery { Filters = filters };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    /// <summary>
    /// Get a product by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "products.read")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetProductByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "products.create")]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
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
    /// Update an existing product
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "products.update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand command)
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
    /// Delete a product (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "products.delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteProductCommand { Id = id };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { message = "Product deleted successfully", success = true });
    }

    /// <summary>
    /// Get warehouse inventory for a specific product
    /// </summary>
    [HttpGet("{id:guid}/inventory")]
    [Authorize(Policy = "products.read")]
    public async Task<IActionResult> GetInventory(Guid id)
    {
        var query = new GetProductInventoryQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }
}
