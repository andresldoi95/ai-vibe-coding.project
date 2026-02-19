using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.Warehouses.Queries.GetWarehouseById;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.Warehouses.Queries;

public class GetWarehouseByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetWarehouseByIdQueryHandler>> _loggerMock;
    private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
    private readonly GetWarehouseByIdQueryHandler _handler;

    public GetWarehouseByIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetWarehouseByIdQueryHandler>>();
        _warehouseRepositoryMock = new Mock<IWarehouseRepository>();

        _unitOfWorkMock.Setup(u => u.Warehouses).Returns(_warehouseRepositoryMock.Object);

        _handler = new GetWarehouseByIdQueryHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnWarehouse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var countryId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var warehouse = new Warehouse
        {
            Id = warehouseId,
            TenantId = tenantId,
            Name = "Main Warehouse",
            Code = "WH-001",
            Description = "Primary storage",
            StreetAddress = "123 Storage St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            CountryId = countryId,
            Country = new Country { Id = countryId, Name = "United States", Code = "US" },
            Phone = "+1-555-0100",
            Email = "warehouse@example.com",
            IsActive = true,
            SquareFootage = 50000m,
            Capacity = 10000,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow
        };

        _warehouseRepositoryMock
            .Setup(r => r.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        var query = new GetWarehouseByIdQuery { Id = warehouseId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(warehouseId);
        result.Value.Name.Should().Be("Main Warehouse");
        result.Value.Code.Should().Be("WH-001");
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.Description.Should().Be("Primary storage");
        result.Value.StreetAddress.Should().Be("123 Storage St");
        result.Value.City.Should().Be("New York");
        result.Value.State.Should().Be("NY");
        result.Value.PostalCode.Should().Be("10001");
        result.Value.CountryName.Should().Be("United States");
        result.Value.Phone.Should().Be("+1-555-0100");
        result.Value.Email.Should().Be("warehouse@example.com");
        result.Value.IsActive.Should().BeTrue();
        result.Value.SquareFootage.Should().Be(50000m);
        result.Value.Capacity.Should().Be(10000);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var query = new GetWarehouseByIdQuery { Id = Guid.NewGuid() };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

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

        var query = new GetWarehouseByIdQuery { Id = warehouseId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

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

        var query = new GetWarehouseByIdQuery { Id = warehouseId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

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

        var query = new GetWarehouseByIdQuery { Id = warehouseId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access denied");
    }

    [Fact]
    public async Task Handle_WarehouseWithNullOptionalFields_ShouldReturnCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var countryId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var warehouse = new Warehouse
        {
            Id = warehouseId,
            TenantId = tenantId,
            Name = "Minimal Warehouse",
            Code = "WH-MIN",
            Description = null,
            StreetAddress = "456 Simple Ave",
            City = "Boston",
            State = null,
            PostalCode = "02101",
            CountryId = countryId,
            Country = new Country { Id = countryId, Name = "United States", Code = "US" },
            Phone = null,
            Email = null,
            IsActive = true,
            SquareFootage = null,
            Capacity = null,
            IsDeleted = false
        };

        _warehouseRepositoryMock
            .Setup(r => r.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        var query = new GetWarehouseByIdQuery { Id = warehouseId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

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
    public async Task Handle_SuccessfulRetrieval_ShouldLogInformation()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var countryId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var warehouse = new Warehouse
        {
            Id = warehouseId,
            TenantId = tenantId,
            Name = "Test Warehouse",
            Code = "WH-LOG",
            StreetAddress = "123 Test St",
            City = "Test City",
            PostalCode = "12345",
            CountryId = countryId,
            Country = new Country { Id = countryId, Name = "United States", Code = "US" },
            IsDeleted = false
        };

        _warehouseRepositoryMock
            .Setup(r => r.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        var query = new GetWarehouseByIdQuery { Id = warehouseId };

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrieved")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
