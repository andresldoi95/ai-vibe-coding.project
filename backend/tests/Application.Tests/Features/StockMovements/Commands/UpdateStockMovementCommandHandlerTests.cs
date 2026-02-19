using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.StockMovements.Commands.UpdateStockMovement;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Services;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace Application.Tests.Features.StockMovements.Commands;

public class UpdateStockMovementCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IStockLevelService> _stockLevelServiceMock;
    private readonly Mock<ILogger<UpdateStockMovementCommandHandler>> _loggerMock;
    private readonly Mock<IStockMovementRepository> _stockMovementRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
    private readonly UpdateStockMovementCommandHandler _handler;

    public UpdateStockMovementCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _stockLevelServiceMock = new Mock<IStockLevelService>();
        _loggerMock = new Mock<ILogger<UpdateStockMovementCommandHandler>>();
        _stockMovementRepositoryMock = new Mock<IStockMovementRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _warehouseRepositoryMock = new Mock<IWarehouseRepository>();

        _unitOfWorkMock.Setup(u => u.StockMovements).Returns(_stockMovementRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Warehouses).Returns(_warehouseRepositoryMock.Object);

        _handler = new UpdateStockMovementCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _stockLevelServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateStockMovement()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var stockMovementId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var existingStockMovement = new StockMovement
        {
            Id = stockMovementId,
            TenantId = tenantId,
            MovementType = MovementType.Purchase,
            ProductId = productId,
            WarehouseId = warehouseId,
            Quantity = 50,
            IsDeleted = false
        };

        var product = new Product { Id = productId, TenantId = tenantId, IsDeleted = false };
        var warehouse = new Warehouse { Id = warehouseId, TenantId = tenantId, IsDeleted = false };

        _stockMovementRepositoryMock
            .Setup(r => r.GetByIdAsync(stockMovementId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingStockMovement);

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _warehouseRepositoryMock
            .Setup(r => r.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        _stockLevelServiceMock
            .Setup(s => s.UpdateStockLevelsAsync(It.IsAny<StockMovement>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var updatedMovement = new StockMovement
        {
            Id = stockMovementId,
            TenantId = tenantId,
            ProductId = productId,
            WarehouseId = warehouseId,
            Quantity = 100,
            UnitCost = 12.00m,
            MovementType = MovementType.Purchase,
            Reference = "PO-002",
            Notes = "Updated purchase",
            Product = product,
            Warehouse = warehouse
        };

        _stockMovementRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(stockMovementId, tenantId))
            .ReturnsAsync(updatedMovement);

        var command = new UpdateStockMovementCommand
        {
            Id = stockMovementId,
            MovementType = MovementType.Purchase,
            ProductId = productId,
            WarehouseId = warehouseId,
            Quantity = 100,
            UnitCost = 12.00m,
            MovementDate = DateTime.UtcNow,
            Reference = "PO-002",
            Notes = "Updated purchase"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Quantity.Should().Be(100);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new UpdateStockMovementCommand
        {
            Id = Guid.NewGuid(),
            MovementType = MovementType.Purchase,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            Quantity = 100,
            MovementDate = DateTime.UtcNow
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context");

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_StockMovementNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _stockMovementRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockMovement?)null);

        var command = new UpdateStockMovementCommand
        {
            Id = Guid.NewGuid(),
            MovementType = MovementType.Purchase,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            Quantity = 100,
            MovementDate = DateTime.UtcNow
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_DifferentTenant_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var stockMovementId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var existingStockMovement = new StockMovement
        {
            Id = stockMovementId,
            TenantId = otherTenantId,
            IsDeleted = false
        };

        _stockMovementRepositoryMock
            .Setup(r => r.GetByIdAsync(stockMovementId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingStockMovement);

        var command = new UpdateStockMovementCommand
        {
            Id = stockMovementId,
            MovementType = MovementType.Purchase,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            Quantity = 100,
            MovementDate = DateTime.UtcNow
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }
}
