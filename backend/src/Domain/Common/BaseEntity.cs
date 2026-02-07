namespace SaaS.Domain.Common;

/// <summary>
/// Base entity class with GUID primary key
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
