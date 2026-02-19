using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.Products.Commands.CreateProduct;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Services;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.Products.Commands;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IStockLevelService> _stockLevelServiceMock;
    private readonly Mock<ILogger<CreateProductCommandHandler>> _loggerMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IStockMovementRepository> _stockMovementRepositoryMock;
    private readonly Mock<IWarehouseInventoryRepository> _warehouseInventoryRepositoryMock;
    private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _stockLevelServiceMock = new Mock<IStockLevelService>();
        _loggerMock = new Mock<ILogger<CreateProductCommandHandler>>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _stockMovementRepositoryMock = new Mock<IStockMovementRepository>();
        _warehouseInventoryRepositoryMock = new Mock<IWarehouseInventoryRepository>();
        _warehouseRepositoryMock = new Mock<IWarehouseRepository>();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.StockMovements).Returns(_stockMovementRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.WarehouseInventory).Returns(_warehouseInventoryRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Warehouses).Returns(_warehouseRepositoryMock.Object);

        _handler = new CreateProductCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _stockLevelServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateProduct()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _productRepositoryMock
            .Setup(r => r.GetByCodeAsync("PROD-001", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Code = "PROD-001",
            SKU = "SKU-001",
            Description = "Test Description",
            Category = "Electronics",
            Brand = "Test Brand",
            UnitPrice = 99.99m,
            CostPrice = 50.00m,
            MinimumStockLevel = 10,
            Weight = 1.5m,
            Dimensions = "10x10x10",
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Test Product");
        result.Value.Code.Should().Be("PROD-001");
        result.Value.SKU.Should().Be("SKU-001");
        result.Value.TenantId.Should().Be(tenantId);

        _productRepositoryMock.Verify(r => r.AddAsync(
            It.Is<Product>(p =>
                p.Name == "Test Product" &&
                p.Code == "PROD-001" &&
                p.TenantId == tenantId),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Code = "PROD-001",
            SKU = "SKU-001",
            UnitPrice = 99.99m,
            CostPrice = 50.00m
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context");

        _productRepositoryMock.Verify(r => r.AddAsync(
            It.IsAny<Product>(),
            It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicateCode_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var existingProduct = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = "PROD-001",
            Name = "Existing Product",
            IsDeleted = false
        };

        _productRepositoryMock
            .Setup(r => r.GetByCodeAsync("PROD-001", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        var command = new CreateProductCommand
        {
            Name = "New Product",
            Code = "PROD-001",
            SKU = "SKU-001",
            UnitPrice = 99.99m,
            CostPrice = 50.00m
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");

        _productRepositoryMock.Verify(r => r.AddAsync(
            It.IsAny<Product>(),
            It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInitialInventory_ShouldCreateStockMovement()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _productRepositoryMock
            .Setup(r => r.GetByCodeAsync("PROD-001", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var warehouse = new Warehouse
        {
            Id = warehouseId,
            TenantId = tenantId,
            IsDeleted = false
        };

        _warehouseRepositoryMock
            .Setup(r => r.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        _stockLevelServiceMock
            .Setup(s => s.UpdateStockLevelsAsync(It.IsAny<StockMovement>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Code = "PROD-001",
            SKU = "SKU-001",
            UnitPrice = 99.99m,
            CostPrice = 50.00m,
            InitialQuantity = 100,
            InitialWarehouseId = warehouseId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _stockMovementRepositoryMock.Verify(r => r.AddAsync(
            It.Is<StockMovement>(sm =>
                sm.Quantity == 100 &&
                sm.WarehouseId == warehouseId &&
                sm.TenantId == tenantId),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _stockLevelServiceMock.Verify(s => s.UpdateStockLevelsAsync(
            It.IsAny<StockMovement>(),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(3));
    }
}
