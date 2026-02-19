using Xunit;
using FluentAssertions;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace Domain.Tests.Entities;

public class StockMovementTests
{
    [Fact]
    public void StockMovement_ShouldInitialize_WithDefaultValues()
    {
        // Arrange & Act
        var stockMovement = new StockMovement();

        // Assert
        stockMovement.MovementType.Should().Be(default(MovementType));
        stockMovement.ProductId.Should().BeEmpty();
        stockMovement.WarehouseId.Should().BeEmpty();
        stockMovement.DestinationWarehouseId.Should().BeNull();
        stockMovement.Quantity.Should().Be(0);
        stockMovement.UnitCost.Should().BeNull();
        stockMovement.TotalCost.Should().BeNull();
    }

    [Fact]
    public void StockMovement_ShouldSet_AllRequiredProperties()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var destinationWarehouseId = Guid.NewGuid();
        var movementDate = DateTime.UtcNow;

        // Act
        var stockMovement = new StockMovement
        {
            TenantId = tenantId,
            MovementType = MovementType.Purchase,
            ProductId = productId,
            WarehouseId = warehouseId,
            DestinationWarehouseId = destinationWarehouseId,
            Quantity = 100,
            UnitCost = 10.50m,
            TotalCost = 1050.00m,
            MovementDate = movementDate,
            Notes = "Test stock movement"
        };

        // Assert
        stockMovement.TenantId.Should().Be(tenantId);
        stockMovement.MovementType.Should().Be(MovementType.Purchase);
        stockMovement.ProductId.Should().Be(productId);
        stockMovement.WarehouseId.Should().Be(warehouseId);
        stockMovement.DestinationWarehouseId.Should().Be(destinationWarehouseId);
        stockMovement.Quantity.Should().Be(100);
        stockMovement.UnitCost.Should().Be(10.50m);
        stockMovement.TotalCost.Should().Be(1050.00m);
        stockMovement.MovementDate.Should().Be(movementDate);
        stockMovement.Notes.Should().Be("Test stock movement");
    }

    [Fact]
    public void StockMovement_ShouldAllowNull_OptionalProperties()
    {
        // Arrange & Act
        var stockMovement = new StockMovement
        {
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            Quantity = 50,
            DestinationWarehouseId = null,
            UnitCost = null,
            TotalCost = null,
            Notes = null
        };

        // Assert
        stockMovement.DestinationWarehouseId.Should().BeNull();
        stockMovement.UnitCost.Should().BeNull();
        stockMovement.TotalCost.Should().BeNull();
        stockMovement.Notes.Should().BeNull();
    }

    [Theory]
    [InlineData(MovementType.Purchase)]
    [InlineData(MovementType.Sale)]
    [InlineData(MovementType.Transfer)]
    [InlineData(MovementType.Adjustment)]
    [InlineData(MovementType.Return)]
    [InlineData(MovementType.InitialInventory)]
    public void StockMovement_MovementType_CanBeSet(MovementType movementType)
    {
        // Arrange & Act
        var stockMovement = new StockMovement { MovementType = movementType };

        // Assert
        stockMovement.MovementType.Should().Be(movementType);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(-50)]
    public void StockMovement_Quantity_CanBeSet(int quantity)
    {
        // Arrange & Act
        var stockMovement = new StockMovement { Quantity = quantity };

        // Assert
        stockMovement.Quantity.Should().Be(quantity);
    }

    [Fact]
    public void StockMovement_UnitCost_CanBeSetToNull()
    {
        // Arrange & Act
        var stockMovement = new StockMovement { UnitCost = null };

        // Assert
        stockMovement.UnitCost.Should().BeNull();
    }

    [Fact]
    public void StockMovement_UnitCost_CanBeSetToDecimalValue()
    {
        // Arrange & Act
        var stockMovement = new StockMovement { UnitCost = 10.50m };

        // Assert
        stockMovement.UnitCost.Should().Be(10.50m);
    }

    [Fact]
    public void StockMovement_UnitCost_CanBeSetToLargeValue()
    {
        // Arrange & Act
        var stockMovement = new StockMovement { UnitCost = 999.99m };

        // Assert
        stockMovement.UnitCost.Should().Be(999.99m);
    }
}
