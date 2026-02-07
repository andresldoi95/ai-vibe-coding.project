using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Auth.Commands.Login;

/// <summary>
/// Command to authenticate user and return tokens
/// </summary>
public record LoginCommand : IRequest<Result<LoginResponseDto>>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string IpAddress { get; init; } = string.Empty;
}
