using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Roles.Commands.DeleteRole;

public class DeleteRoleCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
}
