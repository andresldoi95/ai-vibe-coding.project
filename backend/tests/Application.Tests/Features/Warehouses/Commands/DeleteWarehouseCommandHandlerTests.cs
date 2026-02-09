using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.Warehouses.Commands.DeleteWarehouse;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.Warehouses.Commands;

public class DeleteWarehouseCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<DeleteWarehouseCommandHandler>> _loggerMock;
    private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
    private readonly DeleteWarehouseCommandHandler _handler;

    public DeleteWarehouseCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<DeleteWarehouseCommandHandler>>();
        _warehouseRepositoryMock = new Mock<IWarehouseRepository>();

        _unitOfWorkMock.Setup(u => u.Warehouses).Returns(_warehouseRepositoryMock.Object);

        _handler = new DeleteWarehouseCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldDeleteWarehouse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var warehouse = new Warehouse
        {
            Id = warehouseId,
            TenantId = tenantId,
            Name = "Test Warehouse",
            Code = "WH-001",
            IsDeleted = false
        };

        _warehouseRepositoryMock
            .Setup(r => r.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        var command = new DeleteWarehouseCommand { Id = warehouseId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        _warehouseRepositoryMock.Verify(
            r => r.DeleteAsync(It.Is<Warehouse>(w => w.Id == warehouseId), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new DeleteWarehouseCommand { Id = Guid.NewGuid() };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");

        _warehouseRepositoryMock.Verify(
            r => r.DeleteAsync(It.IsAny<Warehouse>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WarehouseNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _warehouseRepositoryMock
            .Setup(r => r.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Warehouse?)null);

        var command = new DeleteWarehouseCommand { Id = warehouseId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");

        _warehouseRepositoryMock.Verify(
            r => r.DeleteAsync(It.IsAny<Warehouse>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WarehouseAlreadyDeleted_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var warehouse = new Warehouse
        {
            Id = warehouseId,
            TenantId = tenantId,
            Name = "Deleted Warehouse",
            Code = "DEL",
            IsDeleted = true
        };

        _warehouseRepositoryMock
            .Setup(r => r.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        var command = new DeleteWarehouseCommand { Id = warehouseId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");

        _warehouseRepositoryMock.Verify(
            r => r.DeleteAsync(It.IsAny<Warehouse>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_DifferentTenant_ShouldReturnAccessDenied()
    {
        // Arrange
        var currentTenantId = Guid.NewGuid();
        var warehouseTenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(currentTenantId);

        var warehouse = new Warehouse
        {
            Id = warehouseId,
            TenantId = warehouseTenantId,
            Name = "Other Tenant Warehouse",
            Code = "OTHER",
            IsDeleted = false
        };

        _warehouseRepositoryMock
            .Setup(r => r.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        var command = new DeleteWarehouseCommand { Id = warehouseId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access denied");

        _warehouseRepositoryMock.Verify(
            r => r.DeleteAsync(It.IsAny<Warehouse>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_SuccessfulDeletion_ShouldLogInformation()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var warehouse = new Warehouse
        {
            Id = warehouseId,
            TenantId = tenantId,
            Name = "Test Warehouse",
            Code = "WH-LOG",
            IsDeleted = false
        };

        _warehouseRepositoryMock
            .Setup(r => r.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        var command = new DeleteWarehouseCommand { Id = warehouseId };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("deleted")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
