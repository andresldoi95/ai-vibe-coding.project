using Xunit;
using FluentAssertions;
using SaaS.Domain.Common;

namespace Domain.Tests.Common;

// Concrete implementation for testing
public class TestTenantEntity : TenantEntity { }

public class TenantEntityTests
{
    [Fact]
    public void TenantEntity_ShouldInitialize_WithDefaultValues()
    {
        // Arrange & Act
        var entity = new TestTenantEntity();

        // Assert
        entity.Id.Should().NotBeEmpty();
        entity.TenantId.Should().BeEmpty();
        entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        entity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        entity.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void TenantEntity_TenantId_CanBeSet()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var entity = new TestTenantEntity();

        // Act
        entity.TenantId = tenantId;

        // Assert
        entity.TenantId.Should().Be(tenantId);
        entity.TenantId.Should().NotBeEmpty();
    }

    [Fact]
    public void TenantEntity_InheritsFrom_AuditableEntity()
    {
        // Arrange & Act
        var entity = new TestTenantEntity();

        // Assert
        entity.Should().BeAssignableTo<AuditableEntity>();
    }

    [Fact]
    public void TenantEntity_HasAll_AuditableProperties()
    {
        // Arrange
        var entity = new TestTenantEntity
        {
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test@example.com",
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "admin@example.com",
            IsDeleted = false,
            DeletedAt = null
        };

        // Assert
        entity.CreatedAt.Should().NotBe(default);
        entity.CreatedBy.Should().Be("test@example.com");
        entity.UpdatedAt.Should().NotBe(default);
        entity.UpdatedBy.Should().Be("admin@example.com");
        entity.IsDeleted.Should().BeFalse();
        entity.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void TenantEntity_SupportsMultiTenancy_WithDifferentTenants()
    {
        // Arrange
        var tenant1Id = Guid.NewGuid();
        var tenant2Id = Guid.NewGuid();

        // Act
        var entity1 = new TestTenantEntity { TenantId = tenant1Id };
        var entity2 = new TestTenantEntity { TenantId = tenant2Id };

        // Assert
        entity1.TenantId.Should().NotBe(entity2.TenantId);
        entity1.TenantId.Should().Be(tenant1Id);
        entity2.TenantId.Should().Be(tenant2Id);
    }
}
