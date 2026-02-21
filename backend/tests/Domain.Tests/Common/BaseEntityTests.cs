using Xunit;
using FluentAssertions;
using SaaS.Domain.Common;

namespace Domain.Tests.Common;

// Concrete implementation for testing
public class TestEntity : BaseEntity { }

public class BaseEntityTests
{
    [Fact]
    public void BaseEntity_ShouldInitialize_WithNewGuid()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        entity.Id.Should().NotBeEmpty();
        entity.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void BaseEntity_ShouldGenerate_UniqueIds()
    {
        // Arrange & Act
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        // Assert
        entity1.Id.Should().NotBe(entity2.Id);
    }

    [Fact]
    public void BaseEntity_Id_CanBeSet()
    {
        // Arrange
        var customId = Guid.NewGuid();
        var entity = new TestEntity();

        // Act
        entity.Id = customId;

        // Assert
        entity.Id.Should().Be(customId);
    }
}
