using SaaS.Application.Common.Interfaces;

namespace SaaS.Infrastructure.Persistence;

/// <summary>
/// Tenant context provider for multi-tenancy support
/// </summary>
public class TenantContext : ITenantContext
{
    private Guid? _tenantId;
    private string? _schemaName;

    public Guid? TenantId => _tenantId;
    public string? SchemaName => _schemaName;

    public void SetTenant(Guid tenantId, string schemaName)
    {
        _tenantId = tenantId;
        _schemaName = schemaName;
    }
}
