using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Users.Commands.ChangePassword;

/// <summary>
/// Command to change current user's password
/// </summary>
public record ChangePasswordCommand : IRequest<Result<bool>>
{
    public Guid UserId { get; init; }
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
    public string ConfirmPassword { get; init; } = string.Empty;
}
