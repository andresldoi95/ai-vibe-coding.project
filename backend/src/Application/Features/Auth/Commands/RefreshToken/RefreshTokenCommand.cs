using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Command to refresh access token using refresh token
/// </summary>
public record RefreshTokenCommand : IRequest<Result<string>>
{
    public string RefreshToken { get; init; } = string.Empty;
    public string IpAddress { get; init; } = string.Empty;
}
