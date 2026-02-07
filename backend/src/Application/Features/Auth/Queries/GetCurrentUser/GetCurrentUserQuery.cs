using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Auth.Queries.GetCurrentUser;

/// <summary>
/// Query to get current authenticated user
/// </summary>
public record GetCurrentUserQuery : IRequest<Result<UserDto>>
{
    public Guid UserId { get; init; }
}
