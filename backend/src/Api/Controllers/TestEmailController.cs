using Microsoft.AspNetCore.Mvc;
using SaaS.Api.Controllers;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Domain.Enums;

namespace SaaS.Api.Controllers;

/// <summary>
/// Test controller for email functionality (dev/testing only)
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class TestEmailController : ControllerBase
{
    private readonly IEmailService _emailService;

    public TestEmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    /// <summary>
    /// Send a test email
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendTestEmail([FromBody] SendTestEmailRequest request)
    {
        var message = new EmailMessage
        {
            To = request.ToEmail,
            Subject = request.Subject ?? "Test Email from SaaS Platform",
            BodyHtml = request.BodyHtml ?? "<h1>Test Email</h1><p>This is a test email sent via Mailpit.</p>",
            Type = EmailType.Custom
        };

        var result = await _emailService.SendEmailAsync(message);

        if (result.IsSuccess)
            return Ok(new { message = "Email sent successfully! Check Mailpit at http://localhost:8025" });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Send test invitation email
    /// </summary>
    [HttpPost("send-invitation")]
    public async Task<IActionResult> SendTestInvitation([FromBody] SendInvitationRequest request)
    {
        var result = await _emailService.SendUserInvitationAsync(
            request.Email,
            request.CompanyName,
            Guid.NewGuid().ToString(), // Fake token for testing
            request.TenantId
        );

        if (result.IsSuccess)
            return Ok(new { message = "Invitation email sent! Check Mailpit at http://localhost:8025" });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Send test password reset email
    /// </summary>
    [HttpPost("send-password-reset")]
    public async Task<IActionResult> SendTestPasswordReset([FromBody] string email)
    {
        var result = await _emailService.SendPasswordResetAsync(
            email,
            Guid.NewGuid().ToString() // Fake token for testing
        );

        if (result.IsSuccess)
            return Ok(new { message = "Password reset email sent! Check Mailpit at http://localhost:8025" });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Send test welcome email
    /// </summary>
    [HttpPost("send-welcome")]
    public async Task<IActionResult> SendTestWelcome([FromBody] SendWelcomeRequest request)
    {
        var result = await _emailService.SendWelcomeEmailAsync(
            request.Email,
            request.UserName,
            request.TenantId
        );

        if (result.IsSuccess)
            return Ok(new { message = "Welcome email sent! Check Mailpit at http://localhost:8025" });

        return BadRequest(new { error = result.Error });
    }
}

public record SendTestEmailRequest(string ToEmail, string? Subject = null, string? BodyHtml = null);
public record SendInvitationRequest(string Email, string CompanyName, Guid TenantId);
public record SendWelcomeRequest(string Email, string UserName, Guid TenantId);
