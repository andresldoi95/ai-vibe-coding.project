using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.Products.Queries.GetAllProducts;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.Products.Queries;

public class GetAllProductsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetAllProductsQueryHandler>> _loggerMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IWarehouseInventoryRepository> _warehouseInventoryRepositoryMock;
    private readonly GetAllProductsQueryHandler _handler;

    public GetAllProductsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetAllProductsQueryHandler>>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _warehouseInventoryRepositoryMock = new Mock<IWarehouseInventoryRepository>();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.WarehouseInventory).Returns(_warehouseInventoryRepositoryMock.Object);

        _handler = new GetAllProductsQueryHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnProductList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Product 1", Code = "PROD-001", SKU = "SKU-001", MinimumStockLevel = 10 },
            new Product { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Product 2", Code = "PROD-002", SKU = "SKU-002", MinimumStockLevel = 5 }
        };

        _productRepositoryMock
            .Setup(r => r.GetAllByTenantAsync(tenantId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var stockDict = new Dictionary<Guid, int>
        {
            { products[0].Id, 100 },
            { products[1].Id, 50 }
        };

        _warehouseInventoryRepositoryMock
            .Setup(r => r.GetTotalStockByProductIdsAsync(It.IsAny<List<Guid>>(), tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stockDict);

        var query = new GetAllProductsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Count.Should().Be(2);
        result.Value[0].CurrentStockLevel.Should().Be(100);
        result.Value[1].CurrentStockLevel.Should().Be(50);
    }

    [Fact]
    public async Task Handle_NoProducts_ShouldReturnEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _productRepositoryMock
            .Setup(r => r.GetAllByTenantAsync(tenantId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        _warehouseInventoryRepositoryMock
            .Setup(r => r.GetTotalStockByProductIdsAsync(It.IsAny<List<Guid>>(), tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int>());

        var query = new GetAllProductsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Count.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithFilters_ShouldPassFiltersToRepository()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var filters = new ProductFilters
        {
            SearchTerm = "Test",
            Category = "Electronics",
            IsActive = true
        };

        var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Test Product", Code = "PROD-001", SKU = "SKU-001" }
        };

        _productRepositoryMock
            .Setup(r => r.GetAllByTenantAsync(tenantId, filters, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        _warehouseInventoryRepositoryMock
            .Setup(r => r.GetTotalStockByProductIdsAsync(It.IsAny<List<Guid>>(), tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int> { { products[0].Id, 100 } });

        var query = new GetAllProductsQuery { Filters = filters };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _productRepositoryMock.Verify(r => r.GetAllByTenantAsync(tenantId, filters, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_LowStockFilter_ShouldReturnOnlyLowStockProducts()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Product 1", Code = "PROD-001", SKU = "SKU-001", MinimumStockLevel = 50 },
            new Product { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Product 2", Code = "PROD-002", SKU = "SKU-002", MinimumStockLevel = 10 }
        };

        var filters = new ProductFilters { LowStockOnly = true };

        _productRepositoryMock
            .Setup(r => r.GetAllByTenantAsync(tenantId, filters, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var stockDict = new Dictionary<Guid, int>
        {
            { products[0].Id, 30 },  // Below minimum (50)
            { products[1].Id, 100 }  // Above minimum (10)
        };

        _warehouseInventoryRepositoryMock
            .Setup(r => r.GetTotalStockByProductIdsAsync(It.IsAny<List<Guid>>(), tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stockDict);

        var query = new GetAllProductsQuery { Filters = filters };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(1);
        result.Value[0].Code.Should().Be("PROD-001");
        result.Value[0].CurrentStockLevel.Should().Be(30);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var query = new GetAllProductsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");
    }
}
