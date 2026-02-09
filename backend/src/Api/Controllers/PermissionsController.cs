using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Features.Permissions.Queries.GetAllPermissions;

namespace SaaS.Api.Controllers;

[ApiController]
[Route("api/v1/permissions")]
[Authorize]
public class PermissionsController : BaseController
{
    public PermissionsController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get all available permissions
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "roles.read")]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllPermissionsQuery();
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }
}
