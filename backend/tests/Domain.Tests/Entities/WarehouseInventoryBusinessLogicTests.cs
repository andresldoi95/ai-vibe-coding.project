using Xunit;
using FluentAssertions;
using SaaS.Domain.Entities;

namespace Domain.Tests.Entities;

public class WarehouseInventoryBusinessLogicTests
{
    [Theory]
    [InlineData(100, 0, 100)]   // No reservations
    [InlineData(100, 25, 75)]   // Some reserved
    [InlineData(100, 100, 0)]   // Fully reserved
    [InlineData(50, 10, 40)]    // Partial reservation
    [InlineData(0, 0, 0)]       // Empty inventory
    public void WarehouseInventory_AvailableQuantity_CalculatesCorrectly(int quantity, int reserved, int expected)
    {
        // Arrange
        var inventory = new WarehouseInventory
        {
            Quantity = quantity,
            ReservedQuantity = reserved
        };

        // Act
        var available = inventory.AvailableQuantity;

        // Assert
        available.Should().Be(expected);
    }

    [Fact]
    public void WarehouseInventory_AvailableQuantity_UpdatesWhenQuantityChanges()
    {
        // Arrange
        var inventory = new WarehouseInventory
        {
            Quantity = 100,
            ReservedQuantity = 20
        };

        // Act
        var initial = inventory.AvailableQuantity;
        inventory.Quantity = 150;
        var updated = inventory.AvailableQuantity;

        // Assert
        initial.Should().Be(80);
        updated.Should().Be(130);
    }

    [Fact]
    public void WarehouseInventory_AvailableQuantity_UpdatesWhenReservedChanges()
    {
        // Arrange
        var inventory = new WarehouseInventory
        {
            Quantity = 100,
            ReservedQuantity = 20
        };

        // Act
        var initial = inventory.AvailableQuantity;
        inventory.ReservedQuantity = 50;
        var updated = inventory.AvailableQuantity;

        // Assert
        initial.Should().Be(80);
        updated.Should().Be(50);
    }

    [Fact]
    public void WarehouseInventory_AvailableQuantity_CanBeNegative_WhenOverReserved()
    {
        // Arrange
        var inventory = new WarehouseInventory
        {
            Quantity = 50,
            ReservedQuantity = 75
        };

        // Act
        var available = inventory.AvailableQuantity;

        // Assert
        available.Should().Be(-25);
        available.Should().BeLessThan(0);
    }

    [Fact]
    public void WarehouseInventory_LastMovementDate_CanBeNull()
    {
        // Arrange
        var inventory = new WarehouseInventory();

        // Act & Assert
        inventory.LastMovementDate.Should().BeNull();
    }

    [Fact]
    public void WarehouseInventory_LastMovementDate_CanBeSet()
    {
        // Arrange
        var movementDate = DateTime.UtcNow;
        var inventory = new WarehouseInventory
        {
            LastMovementDate = movementDate
        };

        // Act & Assert
        inventory.LastMovementDate.Should().Be(movementDate);
    }

    [Fact]
    public void WarehouseInventory_Quantities_CanBeZero()
    {
        // Arrange & Act
        var inventory = new WarehouseInventory
        {
            Quantity = 0,
            ReservedQuantity = 0
        };

        // Assert
        inventory.Quantity.Should().Be(0);
        inventory.ReservedQuantity.Should().Be(0);
        inventory.AvailableQuantity.Should().Be(0);
    }

    [Fact]
    public void WarehouseInventory_HasProductAndWarehouseReferences()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();

        // Act
        var inventory = new WarehouseInventory
        {
            ProductId = productId,
            WarehouseId = warehouseId
        };

        // Assert
        inventory.ProductId.Should().Be(productId);
        inventory.WarehouseId.Should().Be(warehouseId);
        inventory.ProductId.Should().NotBeEmpty();
        inventory.WarehouseId.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(1000, 100, 900)]
    [InlineData(500, 500, 0)]
    [InlineData(1, 0, 1)]
    public void WarehouseInventory_AvailableQuantity_WorksWithLargeNumbers(int quantity, int reserved, int expected)
    {
        // Arrange
        var inventory = new WarehouseInventory
        {
            Quantity = quantity,
            ReservedQuantity = reserved
        };

        // Act
        var available = inventory.AvailableQuantity;

        // Assert
        available.Should().Be(expected);
    }
}
