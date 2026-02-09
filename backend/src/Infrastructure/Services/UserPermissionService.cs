using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;

namespace SaaS.Infrastructure.Services;

/// <summary>
/// Service for checking user permissions
/// </summary>
public class UserPermissionService : IUserPermissionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserPermissionService> _logger;

    public UserPermissionService(
        IUnitOfWork _unitOfWork,
        ILogger<UserPermissionService> logger)
    {
        this._unitOfWork = _unitOfWork;
        _logger = logger;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, Guid tenantId, string permissionName, CancellationToken cancellationToken = default)
    {
        var userTenant = await _unitOfWork.UserTenants.GetWithRoleAndPermissionsAsync(
            userId,
            tenantId,
            cancellationToken);

        if (userTenant?.Role == null)
        {
            _logger.LogWarning("User {UserId} has no role in tenant {TenantId}", userId, tenantId);
            return false;
        }

        var hasPermission = userTenant.Role.RolePermissions
            .Any(rp => rp.Permission.Name == permissionName);

        return hasPermission;
    }

    public async Task<List<string>> GetUserPermissionsAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var userTenant = await _unitOfWork.UserTenants.GetWithRoleAndPermissionsAsync(
            userId,
            tenantId,
            cancellationToken);

        if (userTenant?.Role == null)
        {
            _logger.LogWarning("User {UserId} has no role in tenant {TenantId}", userId, tenantId);
            return new List<string>();
        }

        var permissions = userTenant.Role.RolePermissions
            .Select(rp => rp.Permission.Name)
            .ToList();

        return permissions;
    }
}
