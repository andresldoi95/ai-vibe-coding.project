namespace SaaS.Application.Common.Interfaces;

/// <summary>
/// Provides the current tenant and user context
/// </summary>
public interface ITenantContext
{
    Guid? TenantId { get; }
    Guid? UserId { get; }
    string? SchemaName { get; }
    void SetTenant(Guid tenantId, string schemaName);
    void SetUser(Guid userId);
}
