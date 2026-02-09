namespace SaaS.Application.Common.Interfaces;

/// <summary>
/// Service for checking user permissions
/// </summary>
public interface IUserPermissionService
{
    /// <summary>
    /// Check if a user has a specific permission for a tenant
    /// </summary>
    Task<bool> HasPermissionAsync(Guid userId, Guid tenantId, string permissionName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all permissions for a user in a specific tenant
    /// </summary>
    Task<List<string>> GetUserPermissionsAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
}
