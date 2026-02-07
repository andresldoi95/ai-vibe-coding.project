using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Features.Tenants.Queries.GetUserTenants;

namespace SaaS.Api.Controllers;

[ApiController]
[Route("api/v1/tenants")]
[Authorize]
public class TenantsController : BaseController
{
    public TenantsController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get all tenants for the current user
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUserTenants()
    {
        var userId = GetUserId();
        var query = new GetUserTenantsQuery { UserId = userId };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(new { data = result.Value, success = true });
    }
}
