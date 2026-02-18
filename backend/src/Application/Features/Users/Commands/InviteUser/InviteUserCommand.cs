using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Users.Commands.InviteUser;

/// <summary>
/// Command to invite a user to the current company
/// </summary>
public record InviteUserCommand : IRequest<Result<Unit>>
{
    public string Email { get; init; } = string.Empty;
    public Guid RoleId { get; init; }
    public string? PersonalMessage { get; init; }
}
