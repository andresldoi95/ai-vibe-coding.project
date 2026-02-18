using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Users.Commands.RemoveUserFromCompany;

/// <summary>
/// Command to remove a user from the current company
/// </summary>
public record RemoveUserFromCompanyCommand(Guid UserId) : IRequest<Result<Unit>>;
