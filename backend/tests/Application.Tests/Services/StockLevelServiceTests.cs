using Xunit;
using FluentAssertions;
using Moq;
using SaaS.Application.Services;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace Application.Tests.Services;

public class StockLevelServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IWarehouseInventoryRepository> _warehouseInventoryRepositoryMock;
    private readonly StockLevelService _service;

    public StockLevelServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _warehouseInventoryRepositoryMock = new Mock<IWarehouseInventoryRepository>();

        // Setup UnitOfWork to return mocked repository
        _unitOfWorkMock.Setup(u => u.WarehouseInventory).Returns(_warehouseInventoryRepositoryMock.Object);

        _service = new StockLevelService(_unitOfWorkMock.Object);
    }

    #region UpdateStockLevelsAsync - InitialInventory Tests

    [Fact]
    public async Task UpdateStockLevelsAsync_InitialInventory_AddsStockToWarehouse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var movementDate = DateTime.UtcNow;
        var quantity = 100;

        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            TenantId = tenantId,
            MovementType = MovementType.InitialInventory,
            Quantity = quantity,
            MovementDate = movementDate
        };

        // Act
        await _service.UpdateStockLevelsAsync(movement, CancellationToken.None);

        // Assert
        _warehouseInventoryRepositoryMock.Verify(
            r => r.UpsertAsync(
                productId,
                warehouseId,
                tenantId,
                100, // Should add positive quantity
                movementDate,
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task UpdateStockLevelsAsync_InitialInventoryWithNegativeQuantity_AddsAbsoluteValue()
    {
        // Arrange - Even if accidentally negative, InitialInventory should add stock
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var movementDate = DateTime.UtcNow;

        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            TenantId = tenantId,
            MovementType = MovementType.InitialInventory,
            Quantity = -100, // Negative quantity
            MovementDate = movementDate
        };

        // Act
        await _service.UpdateStockLevelsAsync(movement, CancellationToken.None);

        // Assert
        _warehouseInventoryRepositoryMock.Verify(
            r => r.UpsertAsync(
                productId,
                warehouseId,
                tenantId,
                100, // Should add absolute value
                movementDate,
                CancellationToken.None),
            Times.Once);
    }

    #endregion

    #region UpdateStockLevelsAsync - Purchase Tests

    [Fact]
    public async Task UpdateStockLevelsAsync_Purchase_AddsStockToWarehouse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var movementDate = DateTime.UtcNow;
        var quantity = 50;

        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            TenantId = tenantId,
            MovementType = MovementType.Purchase,
            Quantity = quantity,
            MovementDate = movementDate
        };

        // Act
        await _service.UpdateStockLevelsAsync(movement, CancellationToken.None);

        // Assert
        _warehouseInventoryRepositoryMock.Verify(
            r => r.UpsertAsync(
                productId,
                warehouseId,
                tenantId,
                50, // Should add quantity
                movementDate,
                CancellationToken.None),
            Times.Once);
    }

    #endregion

    #region UpdateStockLevelsAsync - Return Tests

    [Fact]
    public async Task UpdateStockLevelsAsync_Return_AddsStockToWarehouse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var movementDate = DateTime.UtcNow;
        var quantity = 25;

        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            TenantId = tenantId,
            MovementType = MovementType.Return,
            Quantity = quantity,
            MovementDate = movementDate
        };

        // Act
        await _service.UpdateStockLevelsAsync(movement, CancellationToken.None);

        // Assert
        _warehouseInventoryRepositoryMock.Verify(
            r => r.UpsertAsync(
                productId,
                warehouseId,
                tenantId,
                25, // Should add quantity
                movementDate,
                CancellationToken.None),
            Times.Once);
    }

    #endregion

    #region UpdateStockLevelsAsync - Sale Tests

    [Fact]
    public async Task UpdateStockLevelsAsync_Sale_RemovesStockFromWarehouse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var movementDate = DateTime.UtcNow;
        var quantity = 30;

        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            TenantId = tenantId,
            MovementType = MovementType.Sale,
            Quantity = quantity,
            MovementDate = movementDate
        };

        // Act
        await _service.UpdateStockLevelsAsync(movement, CancellationToken.None);

        // Assert
        _warehouseInventoryRepositoryMock.Verify(
            r => r.UpsertAsync(
                productId,
                warehouseId,
                tenantId,
                -30, // Should subtract quantity
                movementDate,
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task UpdateStockLevelsAsync_SaleWithNegativeQuantity_RemovesAbsoluteValue()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var movementDate = DateTime.UtcNow;

        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            TenantId = tenantId,
            MovementType = MovementType.Sale,
            Quantity = -30, // Negative quantity
            MovementDate = movementDate
        };

        // Act
        await _service.UpdateStockLevelsAsync(movement, CancellationToken.None);

        // Assert
        _warehouseInventoryRepositoryMock.Verify(
            r => r.UpsertAsync(
                productId,
                warehouseId,
                tenantId,
                -30, // Should subtract absolute value
                movementDate,
                CancellationToken.None),
            Times.Once);
    }

    #endregion

    #region UpdateStockLevelsAsync - Transfer Tests

    [Fact]
    public async Task UpdateStockLevelsAsync_Transfer_RemovesFromSourceAndAddsToDestination()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var sourceWarehouseId = Guid.NewGuid();
        var destinationWarehouseId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var movementDate = DateTime.UtcNow;
        var quantity = 40;

        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = sourceWarehouseId,
            DestinationWarehouseId = destinationWarehouseId,
            TenantId = tenantId,
            MovementType = MovementType.Transfer,
            Quantity = quantity,
            MovementDate = movementDate
        };

        // Act
        await _service.UpdateStockLevelsAsync(movement, CancellationToken.None);

        // Assert
        // Should subtract from source
        _warehouseInventoryRepositoryMock.Verify(
            r => r.UpsertAsync(
                productId,
                sourceWarehouseId,
                tenantId,
                -40,
                movementDate,
                CancellationToken.None),
            Times.Once);

        // Should add to destination
        _warehouseInventoryRepositoryMock.Verify(
            r => r.UpsertAsync(
                productId,
                destinationWarehouseId,
                tenantId,
                40,
                movementDate,
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task UpdateStockLevelsAsync_TransferWithoutDestination_OnlyUpdatesSource()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var sourceWarehouseId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var movementDate = DateTime.UtcNow;
        var quantity = 40;

        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = sourceWarehouseId,
            DestinationWarehouseId = null, // No destination
            TenantId = tenantId,
            MovementType = MovementType.Transfer,
            Quantity = quantity,
            MovementDate = movementDate
        };

        // Act
        await _service.UpdateStockLevelsAsync(movement, CancellationToken.None);

        // Assert
        // Should subtract from source
        _warehouseInventoryRepositoryMock.Verify(
            r => r.UpsertAsync(
                productId,
                sourceWarehouseId,
                tenantId,
                -40,
                movementDate,
                CancellationToken.None),
            Times.Once);

        // Should NOT call destination
        _warehouseInventoryRepositoryMock.Verify(
            r => r.UpsertAsync(
                productId,
                It.Is<Guid>(id => id != sourceWarehouseId),
                tenantId,
                It.IsAny<int>(),
                movementDate,
                CancellationToken.None),
            Times.Never);
    }

    #endregion

    #region UpdateStockLevelsAsync - Adjustment Tests

    [Fact]
    public async Task UpdateStockLevelsAsync_AdjustmentPositive_AddsStockToWarehouse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var movementDate = DateTime.UtcNow;
        var quantity = 15;

        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            TenantId = tenantId,
            MovementType = MovementType.Adjustment,
            Quantity = quantity, // Positive adjustment
            MovementDate = movementDate
        };

        // Act
        await _service.UpdateStockLevelsAsync(movement, CancellationToken.None);

        // Assert
        _warehouseInventoryRepositoryMock.Verify(
            r => r.UpsertAsync(
                productId,
                warehouseId,
                tenantId,
                15, // Should use quantity as-is
                movementDate,
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task UpdateStockLevelsAsync_AdjustmentNegative_RemovesStockFromWarehouse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var movementDate = DateTime.UtcNow;
        var quantity = -20;

        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            TenantId = tenantId,
            MovementType = MovementType.Adjustment,
            Quantity = quantity, // Negative adjustment
            MovementDate = movementDate
        };

        // Act
        await _service.UpdateStockLevelsAsync(movement, CancellationToken.None);

        // Assert
        _warehouseInventoryRepositoryMock.Verify(
            r => r.UpsertAsync(
                productId,
                warehouseId,
                tenantId,
                -20, // Should use quantity as-is
                movementDate,
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task UpdateStockLevelsAsync_AdjustmentZero_DoesNothing()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var movementDate = DateTime.UtcNow;

        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            TenantId = tenantId,
            MovementType = MovementType.Adjustment,
            Quantity = 0, // Zero adjustment
            MovementDate = movementDate
        };

        // Act
        await _service.UpdateStockLevelsAsync(movement, CancellationToken.None);

        // Assert - Should not call UpsertAsync when quantity delta is 0
        _warehouseInventoryRepositoryMock.Verify(
            r => r.UpsertAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region ReverseStockLevelsAsync Tests

    [Fact]
    public async Task ReverseStockLevelsAsync_Purchase_SubtractsInsteadOfAdds()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var movementDate = DateTime.UtcNow;
        var quantity = 50;

        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            TenantId = tenantId,
            MovementType = MovementType.Purchase,
            Quantity = quantity,
            MovementDate = movementDate
        };

        // Act
        await _service.ReverseStockLevelsAsync(movement, CancellationToken.None);

        // Assert
        _warehouseInventoryRepositoryMock.Verify(
            r => r.UpsertAsync(
                productId,
                warehouseId,
                tenantId,
                -50, // Should reverse: negate the positive delta
                movementDate,
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task ReverseStockLevelsAsync_Sale_AddsInsteadOfSubtracts()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var movementDate = DateTime.UtcNow;
        var quantity = 30;

        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            TenantId = tenantId,
            MovementType = MovementType.Sale,
            Quantity = quantity,
            MovementDate = movementDate
        };

        // Act
        await _service.ReverseStockLevelsAsync(movement, CancellationToken.None);

        // Assert
        _warehouseInventoryRepositoryMock.Verify(
            r => r.UpsertAsync(
                productId,
                warehouseId,
                tenantId,
                30, // Should reverse: negate the negative delta
                movementDate,
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task ReverseStockLevelsAsync_Transfer_AddsToSourceAndSubtractsFromDestination()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var sourceWarehouseId = Guid.NewGuid();
        var destinationWarehouseId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var movementDate = DateTime.UtcNow;
        var quantity = 40;

        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = sourceWarehouseId,
            DestinationWarehouseId = destinationWarehouseId,
            TenantId = tenantId,
            MovementType = MovementType.Transfer,
            Quantity = quantity,
            MovementDate = movementDate
        };

        // Act
        await _service.ReverseStockLevelsAsync(movement, CancellationToken.None);

        // Assert
        // Should add to source (reverse of subtract)
        _warehouseInventoryRepositoryMock.Verify(
            r => r.UpsertAsync(
                productId,
                sourceWarehouseId,
                tenantId,
                40, // Reversed
                movementDate,
                CancellationToken.None),
            Times.Once);

        // Should subtract from destination (reverse of add)
        _warehouseInventoryRepositoryMock.Verify(
            r => r.UpsertAsync(
                productId,
                destinationWarehouseId,
                tenantId,
                -40, // Reversed
                movementDate,
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task ReverseStockLevelsAsync_AdjustmentPositive_ReversesToNegative()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var movementDate = DateTime.UtcNow;
        var quantity = 15;

        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            TenantId = tenantId,
            MovementType = MovementType.Adjustment,
            Quantity = quantity,
            MovementDate = movementDate
        };

        // Act
        await _service.ReverseStockLevelsAsync(movement, CancellationToken.None);

        // Assert
        _warehouseInventoryRepositoryMock.Verify(
            r => r.UpsertAsync(
                productId,
                warehouseId,
                tenantId,
                -15, // Reversed
                movementDate,
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task ReverseStockLevelsAsync_AdjustmentNegative_ReversesToPositive()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var movementDate = DateTime.UtcNow;
        var quantity = -20;

        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            TenantId = tenantId,
            MovementType = MovementType.Adjustment,
            Quantity = quantity,
            MovementDate = movementDate
        };

        // Act
        await _service.ReverseStockLevelsAsync(movement, CancellationToken.None);

        // Assert
        _warehouseInventoryRepositoryMock.Verify(
            r => r.UpsertAsync(
                productId,
                warehouseId,
                tenantId,
                20, // Reversed
                movementDate,
                CancellationToken.None),
            Times.Once);
    }

    #endregion

    #region CancellationToken Tests

    [Fact]
    public async Task UpdateStockLevelsAsync_WithCancellationToken_PassesTokenToRepository()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var movementDate = DateTime.UtcNow;

        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            TenantId = tenantId,
            MovementType = MovementType.Purchase,
            Quantity = 100,
            MovementDate = movementDate
        };

        var cancellationToken = new CancellationToken();

        // Act
        await _service.UpdateStockLevelsAsync(movement, cancellationToken);

        // Assert
        _warehouseInventoryRepositoryMock.Verify(
            r => r.UpsertAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<DateTime>(),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task ReverseStockLevelsAsync_WithCancellationToken_PassesTokenToRepository()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var movementDate = DateTime.UtcNow;

        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            TenantId = tenantId,
            MovementType = MovementType.Purchase,
            Quantity = 100,
            MovementDate = movementDate
        };

        var cancellationToken = new CancellationToken();

        // Act
        await _service.ReverseStockLevelsAsync(movement, cancellationToken);

        // Assert
        _warehouseInventoryRepositoryMock.Verify(
            r => r.UpsertAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<DateTime>(),
                cancellationToken),
            Times.Once);
    }

    #endregion
}
