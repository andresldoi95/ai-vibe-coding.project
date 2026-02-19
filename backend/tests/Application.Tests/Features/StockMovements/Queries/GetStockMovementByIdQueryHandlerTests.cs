using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.StockMovements.Queries.GetStockMovementById;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace Application.Tests.Features.StockMovements.Queries;

public class GetStockMovementByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetStockMovementByIdQueryHandler>> _loggerMock;
    private readonly Mock<IStockMovementRepository> _stockMovementRepositoryMock;
    private readonly GetStockMovementByIdQueryHandler _handler;

    public GetStockMovementByIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetStockMovementByIdQueryHandler>>();
        _stockMovementRepositoryMock = new Mock<IStockMovementRepository>();

        _unitOfWorkMock.Setup(u => u.StockMovements).Returns(_stockMovementRepositoryMock.Object);

        _handler = new GetStockMovementByIdQueryHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnStockMovement()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var stockMovementId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var stockMovement = new StockMovement
        {
            Id = stockMovementId,
            TenantId = tenantId,
            MovementType = MovementType.Purchase,
            ProductId = productId,
            WarehouseId = warehouseId,
            Quantity = 100,
            UnitCost = 10.50m,
            TotalCost = 1050.00m,
            Reference = "PO-001",
            Notes = "Test movement",
            IsDeleted = false,
            Product = new Product { Id = productId, Name = "Test Product", Code = "PROD-001" },
            Warehouse = new Warehouse { Id = warehouseId, Name = "Main Warehouse", Code = "WH-001" }
        };

        _stockMovementRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(stockMovementId, tenantId))
            .ReturnsAsync(stockMovement);

        var query = new GetStockMovementByIdQuery(stockMovementId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(stockMovementId);
        result.Value.ProductId.Should().Be(productId);
        result.Value.WarehouseId.Should().Be(warehouseId);
        result.Value.Quantity.Should().Be(100);
        result.Value.ProductName.Should().Be("Test Product");
        result.Value.WarehouseName.Should().Be("Main Warehouse");
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var query = new GetStockMovementByIdQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context");
    }

    [Fact]
    public async Task Handle_StockMovementNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _stockMovementRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), tenantId))
            .ReturnsAsync((StockMovement?)null);

        var query = new GetStockMovementByIdQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

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

        var stockMovement = new StockMovement
        {
            Id = stockMovementId,
            TenantId = otherTenantId,
            IsDeleted = false
        };

        _stockMovementRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(stockMovementId, tenantId))
            .ReturnsAsync(stockMovement);

        var query = new GetStockMovementByIdQuery(stockMovementId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_DeletedStockMovement_ShouldReturnNotFound()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var stockMovementId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var stockMovement = new StockMovement
        {
            Id = stockMovementId,
            TenantId = tenantId,
            IsDeleted = true
        };

        _stockMovementRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(stockMovementId, tenantId))
            .ReturnsAsync((StockMovement?)null); // Repository typically excludes deleted items

        var query = new GetStockMovementByIdQuery(stockMovementId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }
}
