using Microsoft.AspNetCore.Authorization;

namespace SaaS.Api.Authorization;

/// <summary>
/// Requirement for permission-based authorization
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string PermissionName { get; }

    public PermissionRequirement(string permissionName)
    {
        PermissionName = permissionName;
    }
}
