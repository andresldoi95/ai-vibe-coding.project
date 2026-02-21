using Xunit;
using FluentAssertions;
using SaaS.Domain.Common;

namespace Domain.Tests.Common;

// Concrete implementation for testing
public class TestAuditableEntity : AuditableEntity { }

public class AuditableEntityTests
{
    [Fact]
    public void AuditableEntity_ShouldInitialize_WithDefaultValues()
    {
        // Arrange & Act
        var entity = new TestAuditableEntity();

        // Assert
        entity.Id.Should().NotBeEmpty();
        entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        entity.CreatedBy.Should().BeNull();
        entity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        entity.UpdatedBy.Should().BeNull();
        entity.DeletedAt.Should().BeNull();
        entity.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void AuditableEntity_CreatedAt_IsSetToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var entity = new TestAuditableEntity();
        var after = DateTime.UtcNow;

        // Assert
        entity.CreatedAt.Should().BeOnOrAfter(before);
        entity.CreatedAt.Should().BeOnOrBefore(after);
        entity.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void AuditableEntity_UpdatedAt_IsSetToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var entity = new TestAuditableEntity();
        var after = DateTime.UtcNow;

        // Assert
        entity.UpdatedAt.Should().BeOnOrAfter(before);
        entity.UpdatedAt.Should().BeOnOrBefore(after);
        entity.UpdatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void AuditableEntity_CreatedBy_CanBeSet()
    {
        // Arrange
        var entity = new TestAuditableEntity();
        var userId = "user@example.com";

        // Act
        entity.CreatedBy = userId;

        // Assert
        entity.CreatedBy.Should().Be(userId);
    }

    [Fact]
    public void AuditableEntity_UpdatedBy_CanBeSet()
    {
        // Arrange
        var entity = new TestAuditableEntity();
        var userId = "admin@example.com";

        // Act
        entity.UpdatedBy = userId;

        // Assert
        entity.UpdatedBy.Should().Be(userId);
    }

    [Fact]
    public void AuditableEntity_SoftDelete_SetsDeletedAtAndIsDeleted()
    {
        // Arrange
        var entity = new TestAuditableEntity();
        var deletedAt = DateTime.UtcNow;

        // Act
        entity.IsDeleted = true;
        entity.DeletedAt = deletedAt;

        // Assert
        entity.IsDeleted.Should().BeTrue();
        entity.DeletedAt.Should().Be(deletedAt);
    }

    [Fact]
    public void AuditableEntity_Timestamps_CanBeModified()
    {
        // Arrange
        var entity = new TestAuditableEntity();
        var customCreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var customUpdatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        entity.CreatedAt = customCreatedAt;
        entity.UpdatedAt = customUpdatedAt;

        // Assert
        entity.CreatedAt.Should().Be(customCreatedAt);
        entity.UpdatedAt.Should().Be(customUpdatedAt);
    }

    [Fact]
    public void AuditableEntity_IsDeleted_DefaultsToFalse()
    {
        // Arrange & Act
        var entity = new TestAuditableEntity();

        // Assert
        entity.IsDeleted.Should().BeFalse();
        entity.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void AuditableEntity_DeletedAt_CanBeNull_WhenNotDeleted()
    {
        // Arrange & Act
        var entity = new TestAuditableEntity
        {
            IsDeleted = false,
            DeletedAt = null
        };

        // Assert
        entity.IsDeleted.Should().BeFalse();
        entity.DeletedAt.Should().BeNull();
    }
}
