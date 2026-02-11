using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Features.SriConfiguration.Commands.UpdateSriConfiguration;
using SaaS.Application.Features.SriConfiguration.Commands.UploadCertificate;
using SaaS.Application.Features.SriConfiguration.Queries.GetSriConfiguration;

namespace SaaS.Api.Controllers;

[ApiController]
[Route("api/v1/sri-configuration")]
[Authorize]
public class SriConfigurationController : BaseController
{
    public SriConfigurationController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get SRI configuration for the current tenant
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "sri_configuration.read")]
    public async Task<IActionResult> Get()
    {
        var query = new GetSriConfigurationQuery();
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    /// <summary>
    /// Update SRI configuration
    /// </summary>
    [HttpPut]
    [Authorize(Policy = "sri_configuration.update")]
    public async Task<IActionResult> Update([FromBody] UpdateSriConfigurationCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }

    /// <summary>
    /// Upload digital certificate (.p12 file)
    /// </summary>
    [HttpPost("certificate")]
    [Authorize(Policy = "sri_configuration.update")]
    public async Task<IActionResult> UploadCertificate([FromForm] IFormFile certificateFile, [FromForm] string certificatePassword)
    {
        if (certificateFile == null || certificateFile.Length == 0)
        {
            return BadRequest(new { message = "Certificate file is required", success = false });
        }

        if (string.IsNullOrEmpty(certificatePassword))
        {
            return BadRequest(new { message = "Certificate password is required", success = false });
        }

        using var memoryStream = new MemoryStream();
        await certificateFile.CopyToAsync(memoryStream);

        var command = new UploadCertificateCommand
        {
            CertificateFile = memoryStream.ToArray(),
            CertificatePassword = certificatePassword
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, success = false });
        }

        return Ok(new { data = result.Value, success = true });
    }
}
