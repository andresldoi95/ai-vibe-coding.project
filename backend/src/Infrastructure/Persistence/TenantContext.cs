using SaaS.Application.Common.Interfaces;

namespace SaaS.Infrastructure.Persistence;

/// <summary>
/// Tenant and user context provider for multi-tenancy support
/// </summary>
public class TenantContext : ITenantContext
{
    private Guid? _tenantId;
    private Guid? _userId;
    private string? _schemaName;

    public Guid? TenantId => _tenantId;
    public Guid? UserId => _userId;
    public string? SchemaName => _schemaName;

    public void SetTenant(Guid tenantId, string schemaName)
    {
        _tenantId = tenantId;
        _schemaName = schemaName;
    }

    public void SetUser(Guid userId)
    {
        _userId = userId;
    }
}
