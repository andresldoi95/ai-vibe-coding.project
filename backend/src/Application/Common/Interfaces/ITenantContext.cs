namespace SaaS.Application.Common.Interfaces;

/// <summary>
/// Provides the current tenant context
/// </summary>
public interface ITenantContext
{
    Guid? TenantId { get; }
    string? SchemaName { get; }
    void SetTenant(Guid tenantId, string schemaName);
}
