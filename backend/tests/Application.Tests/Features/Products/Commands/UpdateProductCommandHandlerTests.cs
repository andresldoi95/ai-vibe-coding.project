using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.Products.Commands.UpdateProduct;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.Products.Commands;

public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<UpdateProductCommandHandler>> _loggerMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IWarehouseInventoryRepository> _warehouseInventoryRepositoryMock;
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<UpdateProductCommandHandler>>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _warehouseInventoryRepositoryMock = new Mock<IWarehouseInventoryRepository>();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.WarehouseInventory).Returns(_warehouseInventoryRepositoryMock.Object);

        _handler = new UpdateProductCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateProduct()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var existingProduct = new Product
        {
            Id = productId,
            TenantId = tenantId,
            Name = "Old Name",
            Code = "OLD-001",
            SKU = "OLD-SKU",
            UnitPrice = 50.00m,
            CostPrice = 25.00m,
            IsActive = true
        };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _productRepositoryMock
            .Setup(r => r.GetByCodeAsync("PROD-001", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _productRepositoryMock
            .Setup(r => r.GetBySKUAsync("NEW-SKU", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _warehouseInventoryRepositoryMock
            .Setup(r => r.GetTotalStockByProductIdAsync(productId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);

        var command = new UpdateProductCommand
        {
            Id = productId,
            Name = "Updated Product",
            Code = "PROD-001",
            Description = "Updated description",
            SKU = "NEW-SKU",
            Category = "Electronics",
            Brand = "TestBrand",
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
        result.Value!.Name.Should().Be("Updated Product");
        result.Value.Code.Should().Be("PROD-001");
        result.Value.SKU.Should().Be("NEW-SKU");
        result.Value.UnitPrice.Should().Be(99.99m);
        result.Value.CurrentStockLevel.Should().Be(100);

        _productRepositoryMock.Verify(r => r.UpdateAsync(
            It.Is<Product>(p => p.Name == "Updated Product" && p.Code == "PROD-001"),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var command = new UpdateProductCommand
        {
            Id = productId,
            Name = "Updated Product",
            Code = "PROD-001",
            SKU = "SKU-001",
            UnitPrice = 99.99m,
            CostPrice = 50.00m,
            MinimumStockLevel = 10,
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");

        _productRepositoryMock.Verify(r => r.UpdateAsync(
            It.IsAny<Product>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WrongTenant_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var differentTenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var existingProduct = new Product
        {
            Id = productId,
            TenantId = differentTenantId,
            Name = "Product",
            Code = "PROD-001",
            SKU = "SKU-001"
        };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        var command = new UpdateProductCommand
        {
            Id = productId,
            Name = "Updated Product",
            Code = "PROD-001",
            SKU = "SKU-001",
            UnitPrice = 99.99m,
            CostPrice = 50.00m,
            MinimumStockLevel = 10,
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_DuplicateCode_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var otherProductId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var existingProduct = new Product
        {
            Id = productId,
            TenantId = tenantId,
            Name = "Product",
            Code = "OLD-001",
            SKU = "SKU-001"
        };

        var anotherProduct = new Product
        {
            Id = otherProductId,
            TenantId = tenantId,
            Code = "PROD-002",
            SKU = "OTHER-SKU"
        };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _productRepositoryMock
            .Setup(r => r.GetByCodeAsync("PROD-002", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(anotherProduct);

        var command = new UpdateProductCommand
        {
            Id = productId,
            Name = "Updated Product",
            Code = "PROD-002",
            SKU = "SKU-001",
            UnitPrice = 99.99m,
            CostPrice = 50.00m,
            MinimumStockLevel = 10,
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");
    }

    [Fact]
    public async Task Handle_DuplicateSKU_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var otherProductId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var existingProduct = new Product
        {
            Id = productId,
            TenantId = tenantId,
            Name = "Product",
            Code = "PROD-001",
            SKU = "OLD-SKU"
        };

        var anotherProduct = new Product
        {
            Id = otherProductId,
            TenantId = tenantId,
            Code = "PROD-002",
            SKU = "OTHER-SKU"
        };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _productRepositoryMock
            .Setup(r => r.GetByCodeAsync("PROD-001", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _productRepositoryMock
            .Setup(r => r.GetBySKUAsync("OTHER-SKU", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(anotherProduct);

        var command = new UpdateProductCommand
        {
            Id = productId,
            Name = "Updated Product",
            Code = "PROD-001",
            SKU = "OTHER-SKU",
            UnitPrice = 99.99m,
            CostPrice = 50.00m,
            MinimumStockLevel = 10,
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Product",
            Code = "PROD-001",
            SKU = "SKU-001",
            UnitPrice = 99.99m,
            CostPrice = 50.00m,
            MinimumStockLevel = 10,
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");
    }
}
