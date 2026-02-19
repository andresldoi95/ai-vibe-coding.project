using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.StockMovements.Commands.CreateStockMovement;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Services;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace Application.Tests.Features.StockMovements.Commands;

public class CreateStockMovementCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IStockLevelService> _stockLevelServiceMock;
    private readonly Mock<ILogger<CreateStockMovementCommandHandler>> _loggerMock;
    private readonly Mock<IStockMovementRepository> _stockMovementRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
    private readonly CreateStockMovementCommandHandler _handler;

    public CreateStockMovementCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _stockLevelServiceMock = new Mock<IStockLevelService>();
        _loggerMock = new Mock<ILogger<CreateStockMovementCommandHandler>>();
        _stockMovementRepositoryMock = new Mock<IStockMovementRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _warehouseRepositoryMock = new Mock<IWarehouseRepository>();

        _unitOfWorkMock.Setup(u => u.StockMovements).Returns(_stockMovementRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Warehouses).Returns(_warehouseRepositoryMock.Object);

        _handler = new CreateStockMovementCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _stockLevelServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateStockMovement()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var product = new Product { Id = productId, TenantId = tenantId, IsDeleted = false };
        var warehouse = new Warehouse { Id = warehouseId, TenantId = tenantId, IsDeleted = false };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _warehouseRepositoryMock
            .Setup(r => r.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        var createdMovement = new StockMovement
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProductId = productId,
            WarehouseId = warehouseId,
            Quantity = 100,
            UnitCost = 10.50m,
            TotalCost = 1050.00m,
            MovementType = MovementType.Purchase,
            Reference = "PO-001",
            Notes = "Initial purchase",
            Product = product,
            Warehouse = warehouse
        };

        _stockMovementRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), tenantId))
            .ReturnsAsync(createdMovement);

        _stockLevelServiceMock
            .Setup(s => s.UpdateStockLevelsAsync(It.IsAny<StockMovement>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CreateStockMovementCommand
        {
            MovementType = MovementType.Purchase,
            ProductId = productId,
            WarehouseId = warehouseId,
            Quantity = 100,
            UnitCost = 10.50m,
            Reference = "PO-001",
            Notes = "Initial purchase"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ProductId.Should().Be(productId);
        result.Value.WarehouseId.Should().Be(warehouseId);
        result.Value.Quantity.Should().Be(100);

        _stockMovementRepositoryMock.Verify(r => r.AddAsync(
            It.Is<StockMovement>(sm =>
                sm.ProductId == productId &&
                sm.WarehouseId == warehouseId &&
                sm.Quantity == 100 &&
                sm.TenantId == tenantId),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new CreateStockMovementCommand
        {
            MovementType = MovementType.Purchase,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            Quantity = 100
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context");

        _stockMovementRepositoryMock.Verify(r => r.AddAsync(
            It.IsAny<StockMovement>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var command = new CreateStockMovementCommand
        {
            MovementType = MovementType.Purchase,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            Quantity = 100
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Product not found");

        _stockMovementRepositoryMock.Verify(r => r.AddAsync(
            It.IsAny<StockMovement>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ProductDifferentTenant_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var product = new Product { Id = productId, TenantId = otherTenantId, IsDeleted = false };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var command = new CreateStockMovementCommand
        {
            MovementType = MovementType.Purchase,
            ProductId = productId,
            WarehouseId = Guid.NewGuid(),
            Quantity = 100
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Product not found");
    }

    [Fact]
    public async Task Handle_WarehouseNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var product = new Product { Id = productId, TenantId = tenantId, IsDeleted = false };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _warehouseRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Warehouse?)null);

        var command = new CreateStockMovementCommand
        {
            MovementType = MovementType.Purchase,
            ProductId = productId,
            WarehouseId = Guid.NewGuid(),
            Quantity = 100
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Warehouse not found");
    }

    [Fact]
    public async Task Handle_TransferWithDestinationWarehouse_ShouldCreateStockMovement()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var sourceWarehouseId = Guid.NewGuid();
        var destinationWarehouseId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var product = new Product { Id = productId, TenantId = tenantId, IsDeleted = false };
        var sourceWarehouse = new Warehouse { Id = sourceWarehouseId, TenantId = tenantId, IsDeleted = false };
        var destinationWarehouse = new Warehouse { Id = destinationWarehouseId, TenantId = tenantId, IsDeleted = false };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _warehouseRepositoryMock
            .Setup(r => r.GetByIdAsync(sourceWarehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceWarehouse);

        _warehouseRepositoryMock
            .Setup(r => r.GetByIdAsync(destinationWarehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinationWarehouse);

        var createdMovement = new StockMovement
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProductId = productId,
            WarehouseId = sourceWarehouseId,
            DestinationWarehouseId = destinationWarehouseId,
            Quantity = 50,
            MovementType = MovementType.Transfer,
            Notes = "Transfer between warehouses",
            Product = product,
            Warehouse = sourceWarehouse,
            DestinationWarehouse = destinationWarehouse
        };

        _stockMovementRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), tenantId))
            .ReturnsAsync(createdMovement);

        _stockLevelServiceMock
            .Setup(s => s.UpdateStockLevelsAsync(It.IsAny<StockMovement>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CreateStockMovementCommand
        {
            MovementType = MovementType.Transfer,
            ProductId = productId,
            WarehouseId = sourceWarehouseId,
            DestinationWarehouseId = destinationWarehouseId,
            Quantity = 50,
            Notes = "Transfer between warehouses"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.DestinationWarehouseId.Should().Be(destinationWarehouseId);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_DestinationWarehouseNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var destinationWarehouseId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var product = new Product { Id = productId, TenantId = tenantId, IsDeleted = false };
        var warehouse = new Warehouse { Id = warehouseId, TenantId = tenantId, IsDeleted = false };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _warehouseRepositoryMock
            .Setup(r => r.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        _warehouseRepositoryMock
            .Setup(r => r.GetByIdAsync(destinationWarehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Warehouse?)null);

        var command = new CreateStockMovementCommand
        {
            MovementType = MovementType.Transfer,
            ProductId = productId,
            WarehouseId = warehouseId,
            DestinationWarehouseId = destinationWarehouseId,
            Quantity = 50
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Destination warehouse not found");
    }

    [Fact]
    public async Task Handle_WithUnitCostNoTotalCost_ShouldCalculateTotalCost()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var product = new Product { Id = productId, TenantId = tenantId, IsDeleted = false };
        var warehouse = new Warehouse { Id = warehouseId, TenantId = tenantId, IsDeleted = false };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _warehouseRepositoryMock
            .Setup(r => r.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        var createdMovement = new StockMovement
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProductId = productId,
            WarehouseId = warehouseId,
            Quantity = 100,
            UnitCost = 10.50m,
            TotalCost = 1050.00m,
            MovementType = MovementType.Purchase,
            Product = product,
            Warehouse = warehouse
        };

        _stockMovementRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), tenantId))
            .ReturnsAsync(createdMovement);

        _stockLevelServiceMock
            .Setup(s => s.UpdateStockLevelsAsync(It.IsAny<StockMovement>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CreateStockMovementCommand
        {
            MovementType = MovementType.Purchase,
            ProductId = productId,
            WarehouseId = warehouseId,
            Quantity = 100,
            UnitCost = 10.50m,
            TotalCost = null // Should be calculated
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCost.Should().Be(1050.00m); // 100 * 10.50

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
