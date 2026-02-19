using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.StockMovements.Commands.DeleteStockMovement;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Services;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace Application.Tests.Features.StockMovements.Commands;

public class DeleteStockMovementCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IStockLevelService> _stockLevelServiceMock;
    private readonly Mock<ILogger<DeleteStockMovementCommandHandler>> _loggerMock;
    private readonly Mock<IStockMovementRepository> _stockMovementRepositoryMock;
    private readonly DeleteStockMovementCommandHandler _handler;

    public DeleteStockMovementCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _stockLevelServiceMock = new Mock<IStockLevelService>();
        _loggerMock = new Mock<ILogger<DeleteStockMovementCommandHandler>>();
        _stockMovementRepositoryMock = new Mock<IStockMovementRepository>();

        _unitOfWorkMock.Setup(u => u.StockMovements).Returns(_stockMovementRepositoryMock.Object);

        _handler = new DeleteStockMovementCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _stockLevelServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldDeleteStockMovement()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var stockMovementId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var stockMovement = new StockMovement
        {
            Id = stockMovementId,
            TenantId = tenantId,
            MovementType = MovementType.Purchase,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            Quantity = 100,
            IsDeleted = false
        };

        _stockMovementRepositoryMock
            .Setup(r => r.GetByIdAsync(stockMovementId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stockMovement);

        var command = new DeleteStockMovementCommand(stockMovementId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        stockMovement.IsDeleted.Should().BeTrue();
        stockMovement.DeletedAt.Should().NotBeNull();

        _stockLevelServiceMock.Verify(s => s.ReverseStockLevelsAsync(
            It.Is<StockMovement>(sm => sm.Id == stockMovementId),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new DeleteStockMovementCommand(Guid.NewGuid());

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

        var command = new DeleteStockMovementCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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
            .Setup(r => r.GetByIdAsync(stockMovementId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stockMovement);

        var command = new DeleteStockMovementCommand(stockMovementId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
