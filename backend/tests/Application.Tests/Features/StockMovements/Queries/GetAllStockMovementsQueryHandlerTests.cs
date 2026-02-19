using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.StockMovements.Queries.GetAllStockMovements;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace Application.Tests.Features.StockMovements.Queries;

public class GetAllStockMovementsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetAllStockMovementsQueryHandler>> _loggerMock;
    private readonly Mock<IStockMovementRepository> _stockMovementRepositoryMock;
    private readonly GetAllStockMovementsQueryHandler _handler;

    public GetAllStockMovementsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetAllStockMovementsQueryHandler>>();
        _stockMovementRepositoryMock = new Mock<IStockMovementRepository>();

        _unitOfWorkMock.Setup(u => u.StockMovements).Returns(_stockMovementRepositoryMock.Object);

        _handler = new GetAllStockMovementsQueryHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnAllStockMovements()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var stockMovements = new List<StockMovement>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                MovementType = MovementType.Purchase,
                ProductId = Guid.NewGuid(),
                WarehouseId = Guid.NewGuid(),
                Quantity = 100,
                IsDeleted = false,
                Product = new Product { Name = "Product 1", Code = "P1" },
                Warehouse = new Warehouse { Name = "Warehouse 1", Code = "W1" }
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                MovementType = MovementType.Sale,
                ProductId = Guid.NewGuid(),
                WarehouseId = Guid.NewGuid(),
                Quantity = -50,
                IsDeleted = false,
                Product = new Product { Name = "Product 2", Code = "P2" },
                Warehouse = new Warehouse { Name = "Warehouse 2", Code = "W2" }
            }
        };

        _stockMovementRepositoryMock
            .Setup(r => r.GetAllForTenantAsync(tenantId))
            .ReturnsAsync(stockMovements);

        var query = new GetAllStockMovementsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Count.Should().Be(2);
        result.Value.Should().Contain(sm => sm.MovementType == MovementType.Purchase);
        result.Value.Should().Contain(sm => sm.MovementType == MovementType.Sale);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var query = new GetAllStockMovementsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context");
    }

    [Fact]
    public async Task Handle_EmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _stockMovementRepositoryMock
            .Setup(r => r.GetAllForTenantAsync(tenantId))
            .ReturnsAsync(new List<StockMovement>());

        var query = new GetAllStockMovementsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldOnlyReturnTenantStockMovements()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var stockMovements = new List<StockMovement>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                MovementType = MovementType.Purchase,
                Quantity = 100,
                IsDeleted = false
            }
        };

        _stockMovementRepositoryMock
            .Setup(r => r.GetAllForTenantAsync(tenantId))
            .ReturnsAsync(stockMovements);

        var query = new GetAllStockMovementsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(1);
        _stockMovementRepositoryMock.Verify(r => r.GetAllForTenantAsync(tenantId), Times.Once);
    }
}
