using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Features.Auth.Commands.ForgotPassword;
using SaaS.Application.Features.Auth.Commands.Login;
using SaaS.Application.Features.Auth.Commands.RefreshToken;
using SaaS.Application.Features.Auth.Commands.Register;
using SaaS.Application.Features.Auth.Commands.ResetPassword;
using SaaS.Application.Features.Auth.Commands.SelectTenant;
using SaaS.Application.Features.Auth.Queries.GetCurrentUser;

namespace SaaS.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : BaseController
{
    public AuthController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Register a new company and admin user
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, errors = result.Errors });
        }

        return Ok(new { data = result.Value, message = "Registration successful", success = true });
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password,
            IpAddress = GetIpAddress()
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return Unauthorized(new { message = result.Error, errors = result.Errors });
        }

        return Ok(new { data = result.Value, message = "Login successful", success = true });
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand
        {
            RefreshToken = request.RefreshToken,
            IpAddress = GetIpAddress()
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return Unauthorized(new { message = result.Error });
        }

        return Ok(new { data = new { accessToken = result.Value }, message = "Token refreshed", success = true });
    }

    /// <summary>
    /// Get current authenticated user
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetUserId();
        var query = new GetCurrentUserQuery { UserId = userId };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.Error });
        }

        return Ok(new { data = result.Value, success = true });
    }

    /// <summary>
    /// Select a tenant and get tenant-scoped JWT with role and permissions
    /// </summary>
    [HttpPost("select-tenant/{tenantId}")]
    [Authorize]
    public async Task<IActionResult> SelectTenant(Guid tenantId)
    {
        var userId = GetUserId();
        var command = new SelectTenantCommand
        {
            UserId = userId,
            TenantId = tenantId
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(new { data = result.Value, message = "Tenant selected successfully", success = true });
    }

    /// <summary>
    /// Request password reset
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var command = new ForgotPasswordCommand
        {
            Email = request.Email
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(new { message = "If the email exists, a password reset link has been sent", success = true });
    }

    /// <summary>
    /// Reset password using token
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var command = new ResetPasswordCommand
        {
            Token = request.Token,
            NewPassword = request.NewPassword,
            ConfirmPassword = request.ConfirmPassword
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, errors = result.Errors });
        }

        return Ok(new { message = "Password reset successfully", success = true });
    }
}

public record LoginRequest(string Email, string Password);
public record RefreshTokenRequest(string RefreshToken);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Token, string NewPassword, string ConfirmPassword);
