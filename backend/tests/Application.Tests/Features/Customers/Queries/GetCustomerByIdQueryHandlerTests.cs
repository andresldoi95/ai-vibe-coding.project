using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.Customers.Queries.GetCustomerById;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.Customers.Queries;

public class GetCustomerByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetCustomerByIdQueryHandler>> _loggerMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly GetCustomerByIdQueryHandler _handler;

    public GetCustomerByIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetCustomerByIdQueryHandler>>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();

        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepositoryMock.Object);

        _handler = new GetCustomerByIdQueryHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidId_ShouldReturnCustomer()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var billingCountryId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var customer = new Customer
        {
            Id = customerId,
            TenantId = tenantId,
            Name = "Test Customer",
            Email = "test@example.com",
            Phone = "+1234567890",
            TaxId = "1234567890",
            ContactPerson = "John Doe",
            BillingStreet = "123 Main St",
            BillingCity = "New York",
            BillingState = "NY",
            BillingPostalCode = "10001",
            BillingCountryId = billingCountryId,
            BillingCountry = new Country { Id = billingCountryId, Name = "United States", Code = "US" },
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var query = new GetCustomerByIdQuery { Id = customerId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(customerId);
        result.Value.Name.Should().Be("Test Customer");
        result.Value.Email.Should().Be("test@example.com");
        result.Value.BillingCountryName.Should().Be("United States");
    }

    [Fact]
    public async Task Handle_CustomerNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var query = new GetCustomerByIdQuery { Id = customerId };

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
        var customerId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var customer = new Customer
        {
            Id = customerId,
            TenantId = differentTenantId,
            Name = "Test Customer",
            Email = "test@example.com"
        };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var query = new GetCustomerByIdQuery { Id = customerId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Unauthorized");
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var query = new GetCustomerByIdQuery { Id = Guid.NewGuid() };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");
    }

    [Fact]
    public async Task Handle_CustomerWithoutCountry_ShouldReturnNullCountryName()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var customer = new Customer
        {
            Id = customerId,
            TenantId = tenantId,
            Name = "Test Customer",
            Email = "test@example.com",
            BillingCountry = null,
            ShippingCountry = null,
            IsActive = true
        };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var query = new GetCustomerByIdQuery { Id = customerId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.BillingCountryName.Should().BeNull();
        result.Value.ShippingCountryName.Should().BeNull();
    }
}
