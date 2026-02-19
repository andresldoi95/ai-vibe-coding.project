using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.Products.Queries.GetProductById;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.Products.Queries;

public class GetProductByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetProductByIdQueryHandler>> _loggerMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IWarehouseInventoryRepository> _warehouseInventoryRepositoryMock;
    private readonly GetProductByIdQueryHandler _handler;

    public GetProductByIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetProductByIdQueryHandler>>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _warehouseInventoryRepositoryMock = new Mock<IWarehouseInventoryRepository>();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.WarehouseInventory).Returns(_warehouseInventoryRepositoryMock.Object);

        _handler = new GetProductByIdQueryHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidId_ShouldReturnProduct()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var product = new Product
        {
            Id = productId,
            TenantId = tenantId,
            Name = "Test Product",
            Code = "PROD-001",
            SKU = "SKU-001",
            Description = "Test description",
            Category = "Electronics",
            Brand = "TestBrand",
            UnitPrice = 99.99m,
            CostPrice = 50.00m,
            MinimumStockLevel = 10,
            Weight = 1.5m,
            Dimensions = "10x10x10",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _warehouseInventoryRepositoryMock
            .Setup(r => r.GetTotalStockByProductIdAsync(productId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(150);

        var query = new GetProductByIdQuery { Id = productId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(productId);
        result.Value.Name.Should().Be("Test Product");
        result.Value.Code.Should().Be("PROD-001");
        result.Value.CurrentStockLevel.Should().Be(150);
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

        var query = new GetProductByIdQuery { Id = productId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_WrongTenant_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var differentTenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var product = new Product
        {
            Id = productId,
            TenantId = differentTenantId,
            Name = "Test Product",
            Code = "PROD-001",
            SKU = "SKU-001"
        };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var query = new GetProductByIdQuery { Id = productId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var query = new GetProductByIdQuery { Id = Guid.NewGuid() };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");
    }

    [Fact]
    public async Task Handle_ProductWithNoStock_ShouldReturnZeroStock()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var product = new Product
        {
            Id = productId,
            TenantId = tenantId,
            Name = "Test Product",
            Code = "PROD-001",
            SKU = "SKU-001",
            UnitPrice = 99.99m,
            CostPrice = 50.00m,
            MinimumStockLevel = 10,
            IsActive = true
        };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _warehouseInventoryRepositoryMock
            .Setup(r => r.GetTotalStockByProductIdAsync(productId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var query = new GetProductByIdQuery { Id = productId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.CurrentStockLevel.Should().Be(0);
    }
}
