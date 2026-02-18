using SaaS.Application.Common.Interfaces;

namespace SaaS.Api.Middleware;

/// <summary>
/// Middleware to resolve tenant context from X-Tenant-Id header
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, IUnitOfWork unitOfWork)
    {
        // Skip tenant resolution for auth endpoints
        if (context.Request.Path.StartsWithSegments("/api/v1/auth"))
        {
            await _next(context);
            return;
        }

        // Skip if user is not authenticated
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        // Extract tenant ID from header
        if (!context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdValue))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { message = "X-Tenant-Id header is required" });
            return;
        }

        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { message = "Invalid X-Tenant-Id format" });
            return;
        }

        // Get user ID from JWT claims
        var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Invalid user token" });
            return;
        }

        // Verify user has access to the tenant
        var userTenants = await unitOfWork.Tenants.GetUserTenantsAsync(userId);
        var tenant = userTenants.FirstOrDefault(t => t.Id == tenantId);

        if (tenant == null)
        {
            _logger.LogWarning("User {UserId} attempted to access unauthorized tenant {TenantId}", userId, tenantId);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { message = "Access denied to this tenant" });
            return;
        }

        // Set tenant context
        tenantContext.SetTenant(tenantId, tenant.SchemaName ?? $"tenant_{tenant.Slug}");
        tenantContext.SetUser(userId);

        _logger.LogDebug("Tenant context set: {TenantId} - {TenantName}, User: {UserId}", tenantId, tenant.Name, userId);

        await _next(context);
    }
}
