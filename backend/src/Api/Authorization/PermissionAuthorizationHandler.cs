using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SaaS.Api.Authorization;

/// <summary>
/// Authorization handler for permission requirements
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Get permission claims from JWT
        var permissions = context.User.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToList();

        if (permissions.Contains(requirement.PermissionName))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
