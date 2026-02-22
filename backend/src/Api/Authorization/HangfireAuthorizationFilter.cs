using Hangfire.Dashboard;

namespace SaaS.Api.Authorization;

/// <summary>
/// Authorization filter for Hangfire dashboard.
/// In development, allows all access. In production, should be restricted to admin users.
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // In development, allow all access to Hangfire dashboard
        // In production, you should check for authentication and admin role
        var isDevelopment = httpContext.RequestServices
            .GetRequiredService<IHostEnvironment>()
            .IsDevelopment();

        if (isDevelopment)
        {
            return true;
        }

        // In production, require authentication and check for admin role
        // Example: return httpContext.User.IsInRole("Admin");

        // For now, allow access in production (you should implement proper authorization)
        return httpContext.User.Identity?.IsAuthenticated ?? false;
    }
}
