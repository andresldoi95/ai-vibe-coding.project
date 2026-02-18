using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Users.Commands.UpdateUserRole;

/// <summary>
/// Command to update a user's role in the current company
/// </summary>
public record UpdateUserRoleCommand : IRequest<Result<Unit>>
{
    public Guid UserId { get; init; }
    public Guid NewRoleId { get; init; }
}
