using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Users.Commands.UpdateCurrentUser;

/// <summary>
/// Command to update current user's profile
/// </summary>
public record UpdateCurrentUserCommand : IRequest<Result<UserDto>>
{
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
}
