using Xunit;
using FluentAssertions;
using SaaS.Domain.Entities;

namespace Domain.Tests.Entities;

public class WarehouseInventoryTests
{
    [Fact]
    public void WarehouseInventory_ShouldInitialize_WithDefaultValues()
    {
        // Arrange & Act
        var inventory = new WarehouseInventory();

        // Assert
        inventory.ProductId.Should().BeEmpty();
        inventory.WarehouseId.Should().BeEmpty();
        inventory.Quantity.Should().Be(0);
        inventory.ReservedQuantity.Should().Be(0);
        inventory.AvailableQuantity.Should().Be(0);
    }

    [Fact]
    public void WarehouseInventory_ShouldSet_AllRequiredProperties()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var lastUpdated = DateTime.UtcNow;

        // Act
        var inventory = new WarehouseInventory
        {
            TenantId = tenantId,
            ProductId = productId,
            WarehouseId = warehouseId,
            Quantity = 100,
            ReservedQuantity = 20,
            LastMovementDate = lastUpdated
        };

        // Assert
        inventory.TenantId.Should().Be(tenantId);
        inventory.ProductId.Should().Be(productId);
        inventory.WarehouseId.Should().Be(warehouseId);
        inventory.Quantity.Should().Be(100);
        inventory.ReservedQuantity.Should().Be(20);
        inventory.LastMovementDate.Should().Be(lastUpdated);
    }

    [Fact]
    public void WarehouseInventory_AvailableQuantity_CalculatesCorrectly()
    {
        // Arrange
        var inventory = new WarehouseInventory
        {
            Quantity = 100,
            ReservedQuantity = 25
        };

        // Act & Assert
        inventory.AvailableQuantity.Should().Be(75);
    }

    [Theory]
    [InlineData(100, 0, 100)]
    [InlineData(100, 50, 50)]
    [InlineData(100, 100, 0)]
    [InlineData(50, 75, -25)]
    public void WarehouseInventory_AvailableQuantity_CalculatesCorrectly_ForDifferentScenarios(
        int quantity, int reserved, int expectedAvailable)
    {
        // Arrange
        var inventory = new WarehouseInventory
        {
            Quantity = quantity,
            ReservedQuantity = reserved
        };

        // Act & Assert
        inventory.AvailableQuantity.Should().Be(expectedAvailable);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void WarehouseInventory_Quantity_CanBeSet(int quantity)
    {
        // Arrange & Act
        var inventory = new WarehouseInventory { Quantity = quantity };

        // Assert
        inventory.Quantity.Should().Be(quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(50)]
    public void WarehouseInventory_ReservedQuantity_CanBeSet(int reserved)
    {
        // Arrange & Act
        var inventory = new WarehouseInventory { ReservedQuantity = reserved };

        // Assert
        inventory.ReservedQuantity.Should().Be(reserved);
    }
}
