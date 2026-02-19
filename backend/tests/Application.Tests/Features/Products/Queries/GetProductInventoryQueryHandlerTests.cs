using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.Products.Queries.GetProductInventory;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.Products.Queries;

public class GetProductInventoryQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetProductInventoryQueryHandler>> _loggerMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IWarehouseInventoryRepository> _warehouseInventoryRepositoryMock;
    private readonly GetProductInventoryQueryHandler _handler;

    public GetProductInventoryQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetProductInventoryQueryHandler>>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _warehouseInventoryRepositoryMock = new Mock<IWarehouseInventoryRepository>();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.WarehouseInventory).Returns(_warehouseInventoryRepositoryMock.Object);

        _handler = new GetProductInventoryQueryHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidProductId_ShouldReturnInventoryList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var warehouse1Id = Guid.NewGuid();
        var warehouse2Id = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var product = new Product
        {
            Id = productId,
            TenantId = tenantId,
            Name = "Test Product",
            Code = "PROD-001"
        };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var inventoryRecords = new List<WarehouseInventory>
        {
            new WarehouseInventory
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProductId = productId,
                WarehouseId = warehouse1Id,
                Quantity = 100,
                ReservedQuantity = 10,
                Product = product,
                Warehouse = new Warehouse { Id = warehouse1Id, Name = "Warehouse 1", Code = "WH-001", TenantId = tenantId }
            },
            new WarehouseInventory
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProductId = productId,
                WarehouseId = warehouse2Id,
                Quantity = 50,
                ReservedQuantity = 5,
                Product = product,
                Warehouse = new Warehouse { Id = warehouse2Id, Name = "Warehouse 2", Code = "WH-002", TenantId = tenantId }
            }
        };

        _warehouseInventoryRepositoryMock
            .Setup(r => r.GetByProductIdAsync(productId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryRecords);

        var query = new GetProductInventoryQuery(productId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Count.Should().Be(2);
        result.Value[0].Quantity.Should().Be(100);
        result.Value[0].AvailableQuantity.Should().Be(90);
        result.Value[0].WarehouseName.Should().Be("Warehouse 1");
        result.Value[1].Quantity.Should().Be(50);
        result.Value[1].AvailableQuantity.Should().Be(45);
        result.Value[1].WarehouseName.Should().Be("Warehouse 2");
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

        var query = new GetProductInventoryQuery(productId);

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
            Code = "PROD-001"
        };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var query = new GetProductInventoryQuery(productId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_ProductWithNoInventory_ShouldReturnEmptyList()
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
            Code = "PROD-001"
        };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _warehouseInventoryRepositoryMock
            .Setup(r => r.GetByProductIdAsync(productId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WarehouseInventory>());

        var query = new GetProductInventoryQuery(productId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Count.Should().Be(0);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var query = new GetProductInventoryQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");
    }
}
