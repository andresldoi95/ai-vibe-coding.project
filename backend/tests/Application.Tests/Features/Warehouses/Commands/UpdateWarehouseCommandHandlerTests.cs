using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.Warehouses.Commands.UpdateWarehouse;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.Warehouses.Commands;

public class UpdateWarehouseCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<UpdateWarehouseCommandHandler>> _loggerMock;
    private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
    private readonly UpdateWarehouseCommandHandler _handler;

    public UpdateWarehouseCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<UpdateWarehouseCommandHandler>>();
        _warehouseRepositoryMock = new Mock<IWarehouseRepository>();

        _unitOfWorkMock.Setup(u => u.Warehouses).Returns(_warehouseRepositoryMock.Object);

        _handler = new UpdateWarehouseCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateWarehouse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var existingWarehouse = new Warehouse
        {
            Id = warehouseId,
            TenantId = tenantId,
            Name = "Old Name",
            Code = "WH-001",
            StreetAddress = "Old Address",
            City = "Old City",
            PostalCode = "00000",
            Country = "USA",
            IsDeleted = false
        };

        _warehouseRepositoryMock
            .Setup(r => r.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingWarehouse);

        var command = new UpdateWarehouseCommand
        {
            Id = warehouseId,
            Name = "Updated Warehouse",
            Code = "WH-001",
            Description = "Updated description",
            StreetAddress = "456 New St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "USA",
            Phone = "+1-555-0100",
            Email = "updated@example.com",
            IsActive = true,
            SquareFootage = 60000m,
            Capacity = 12000
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Updated Warehouse");
        result.Value.Description.Should().Be("Updated description");

        _warehouseRepositoryMock.Verify(r => r.UpdateAsync(
            It.Is<Warehouse>(w => w.Name == "Updated Warehouse"),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new UpdateWarehouseCommand
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Code = "TEST",
            StreetAddress = "123 Test St",
            City = "Test",
            PostalCode = "12345",
            Country = "USA"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");
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

        var command = new UpdateWarehouseCommand
        {
            Id = warehouseId,
            Name = "Test",
            Code = "TEST",
            StreetAddress = "123 Test St",
            City = "Test",
            PostalCode = "12345",
            Country = "USA"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_WarehouseDeleted_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var deletedWarehouse = new Warehouse
        {
            Id = warehouseId,
            TenantId = tenantId,
            Name = "Deleted",
            Code = "DEL",
            IsDeleted = true
        };

        _warehouseRepositoryMock
            .Setup(r => r.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deletedWarehouse);

        var command = new UpdateWarehouseCommand
        {
            Id = warehouseId,
            Name = "Test",
            Code = "TEST",
            StreetAddress = "123 Test St",
            City = "Test",
            PostalCode = "12345",
            Country = "USA"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
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

        var command = new UpdateWarehouseCommand
        {
            Id = warehouseId,
            Name = "Test",
            Code = "TEST",
            StreetAddress = "123 Test St",
            City = "Test",
            PostalCode = "12345",
            Country = "USA"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access denied");
    }

    [Fact]
    public async Task Handle_CodeChangeToDuplicateCode_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var existingWarehouse = new Warehouse
        {
            Id = warehouseId,
            TenantId = tenantId,
            Name = "Warehouse 1",
            Code = "WH-001",
            IsDeleted = false
        };

        var warehouseWithNewCode = new Warehouse
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "Warehouse 2",
            Code = "WH-002",
            IsDeleted = false
        };

        _warehouseRepositoryMock
            .Setup(r => r.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingWarehouse);

        _warehouseRepositoryMock
            .Setup(r => r.GetByCodeAsync("WH-002", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouseWithNewCode);

        var command = new UpdateWarehouseCommand
        {
            Id = warehouseId,
            Name = "Warehouse 1",
            Code = "WH-002", // Trying to change to existing code
            StreetAddress = "123 Test St",
            City = "Test",
            PostalCode = "12345",
            Country = "USA"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("WH-002");
        result.Error.Should().Contain("already exists");
    }

    [Fact]
    public async Task Handle_CodeNotChanged_ShouldNotCheckForDuplicates()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var existingWarehouse = new Warehouse
        {
            Id = warehouseId,
            TenantId = tenantId,
            Name = "Warehouse 1",
            Code = "WH-001",
            StreetAddress = "Old Addr",
            City = "Old City",
            PostalCode = "00000",
            Country = "USA",
            IsDeleted = false
        };

        _warehouseRepositoryMock
            .Setup(r => r.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingWarehouse);

        var command = new UpdateWarehouseCommand
        {
            Id = warehouseId,
            Name = "Updated Name",
            Code = "WH-001", // Same code
            StreetAddress = "New Addr",
            City = "New City",
            PostalCode = "11111",
            Country = "USA"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _warehouseRepositoryMock.Verify(
            r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
