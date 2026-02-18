using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Users.Commands.AcceptInvitation;

/// <summary>
/// Command to accept an invitation and join a company
/// </summary>
public record AcceptInvitationCommand : IRequest<Result<LoginResponseDto>>
{
    public string InvitationToken { get; init; } = string.Empty;
    public string? Name { get; init; }
    public string? Password { get; init; }
}
