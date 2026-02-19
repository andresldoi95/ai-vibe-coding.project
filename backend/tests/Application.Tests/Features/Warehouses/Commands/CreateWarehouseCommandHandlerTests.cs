using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.Warehouses.Commands.CreateWarehouse;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Interfaces;

namespace Application.Tests.Features.Warehouses.Commands;

public class CreateWarehouseCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<CreateWarehouseCommandHandler>> _loggerMock;
    private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
    private readonly Mock<ICountryRepository> _countryRepositoryMock;
    private readonly CreateWarehouseCommandHandler _handler;

    public CreateWarehouseCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<CreateWarehouseCommandHandler>>();
        _warehouseRepositoryMock = new Mock<IWarehouseRepository>();
        _countryRepositoryMock = new Mock<ICountryRepository>();

        // Setup UnitOfWork to return mocked repositories
        _unitOfWorkMock.Setup(u => u.Warehouses).Returns(_warehouseRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Countries).Returns(_countryRepositoryMock.Object);

        _handler = new CreateWarehouseCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateWarehouse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var countryId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _warehouseRepositoryMock
            .Setup(r => r.GetByCodeAsync("WH-001", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Warehouse?)null);

        _countryRepositoryMock
            .Setup(r => r.GetByIdAsync(countryId))
            .ReturnsAsync(new Country { Id = countryId, Name = "United States", Code = "US" });

        var command = new CreateWarehouseCommand
        {
            Name = "Main Warehouse",
            Code = "WH-001",
            Description = "Primary storage facility",
            StreetAddress = "123 Storage St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            CountryId = countryId,
            Phone = "+1-555-0100",
            Email = "warehouse@example.com",
            IsActive = true,
            SquareFootage = 50000m,
            Capacity = 10000
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Main Warehouse");
        result.Value.Code.Should().Be("WH-001");
        result.Value.TenantId.Should().Be(tenantId);

        _warehouseRepositoryMock.Verify(r => r.AddAsync(
            It.Is<Warehouse>(w =>
                w.Name == "Main Warehouse" &&
                w.Code == "WH-001" &&
                w.TenantId == tenantId),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new CreateWarehouseCommand
        {
            Name = "Main Warehouse",
            Code = "WH-001",
            StreetAddress = "123 Storage St",
            City = "New York",
            PostalCode = "10001",
            CountryId = Guid.NewGuid()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");

        _warehouseRepositoryMock.Verify(r => r.AddAsync(
            It.IsAny<Warehouse>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicateWarehouseCode_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var existingWarehouse = new Warehouse
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = "WH-001",
            Name = "Existing Warehouse"
        };

        _warehouseRepositoryMock
            .Setup(r => r.GetByCodeAsync("WH-001", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingWarehouse);

        var command = new CreateWarehouseCommand
        {
            Name = "New Warehouse",
            Code = "WH-001",
            StreetAddress = "123 Storage St",
            City = "New York",
            PostalCode = "10001",
            CountryId = Guid.NewGuid()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("WH-001");
        result.Error.Should().Contain("already exists");

        _warehouseRepositoryMock.Verify(r => r.AddAsync(
            It.IsAny<Warehouse>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ValidCommandWithOptionalFields_ShouldCreateWarehouse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var countryId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _warehouseRepositoryMock
            .Setup(r => r.GetByCodeAsync("WH-MIN", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Warehouse?)null);

        _countryRepositoryMock
            .Setup(r => r.GetByIdAsync(countryId))
            .ReturnsAsync(new Country { Id = countryId, Name = "United States", Code = "US" });

        var command = new CreateWarehouseCommand
        {
            Name = "Minimal Warehouse",
            Code = "WH-MIN",
            Description = null,
            StreetAddress = "456 Simple Ave",
            City = "Boston",
            State = null,
            PostalCode = "02101",
            CountryId = countryId,
            Phone = null,
            Email = null,
            IsActive = true,
            SquareFootage = null,
            Capacity = null
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Description.Should().BeNull();
        result.Value.State.Should().BeNull();
        result.Value.Phone.Should().BeNull();
        result.Value.Email.Should().BeNull();
        result.Value.SquareFootage.Should().BeNull();
        result.Value.Capacity.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldSetAllProperties()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var countryId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _warehouseRepositoryMock
            .Setup(r => r.GetByCodeAsync("WH-FULL", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Warehouse?)null);

        _countryRepositoryMock
            .Setup(r => r.GetByIdAsync(countryId))
            .ReturnsAsync(new Country { Id = countryId, Name = "United States", Code = "US" });

        var command = new CreateWarehouseCommand
        {
            Name = "Full Warehouse",
            Code = "WH-FULL",
            Description = "Complete warehouse details",
            StreetAddress = "789 Complete Blvd",
            City = "Chicago",
            State = "IL",
            PostalCode = "60601",
            CountryId = countryId,
            Phone = "+1-555-0200",
            Email = "full@example.com",
            IsActive = false,
            SquareFootage = 75000m,
            Capacity = 15000
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Full Warehouse");
        result.Value.Code.Should().Be("WH-FULL");
        result.Value.Description.Should().Be("Complete warehouse details");
        result.Value.StreetAddress.Should().Be("789 Complete Blvd");
        result.Value.City.Should().Be("Chicago");
        result.Value.State.Should().Be("IL");
        result.Value.PostalCode.Should().Be("60601");
        result.Value.CountryName.Should().NotBeNullOrEmpty();
        result.Value.Phone.Should().Be("+1-555-0200");
        result.Value.Email.Should().Be("full@example.com");
        result.Value.IsActive.Should().BeFalse();
        result.Value.SquareFootage.Should().Be(75000m);
        result.Value.Capacity.Should().Be(15000);
        result.Value.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task Handle_SuccessfulCreation_ShouldLogInformation()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var countryId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _warehouseRepositoryMock
            .Setup(r => r.GetByCodeAsync("WH-LOG", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Warehouse?)null);

        _countryRepositoryMock
            .Setup(r => r.GetByIdAsync(countryId))
            .ReturnsAsync(new Country { Id = countryId, Name = "United States", Code = "US" });

        var command = new CreateWarehouseCommand
        {
            Name = "Log Test Warehouse",
            Code = "WH-LOG",
            StreetAddress = "123 Log St",
            City = "Test City",
            PostalCode = "12345",
            CountryId = countryId
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("WH-LOG") && v.ToString()!.Contains("created")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
