using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.Customers.Queries.GetAllCustomers;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.DTOs;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.Customers.Queries;

public class GetAllCustomersQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetAllCustomersQueryHandler>> _loggerMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly GetAllCustomersQueryHandler _handler;

    public GetAllCustomersQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetAllCustomersQueryHandler>>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();

        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepositoryMock.Object);

        _handler = new GetAllCustomersQueryHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnCustomerList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var country1 = new Country { Id = Guid.NewGuid(), Name = "Ecuador", Code = "EC" };
        var country2 = new Country { Id = Guid.NewGuid(), Name = "Colombia", Code = "CO" };

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var customers = new List<Customer>
        {
            new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = "Customer 1",
                Email = "customer1@example.com",
                TaxId = "1234567890",
                BillingCountry = country1,
                IsActive = true
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = "Customer 2",
                Email = "customer2@example.com",
                TaxId = "0987654321",
                BillingCountry = country2,
                IsActive = true
            }
        };

        _customerRepositoryMock
            .Setup(r => r.GetAllByTenantAsync(tenantId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customers);

        var query = new GetAllCustomersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Count.Should().Be(2);
        result.Value[0].Name.Should().Be("Customer 1");
        result.Value[0].BillingCountryName.Should().Be("Ecuador");
        result.Value[1].Name.Should().Be("Customer 2");
        result.Value[1].BillingCountryName.Should().Be("Colombia");
    }

    [Fact]
    public async Task Handle_NoCustomers_ShouldReturnEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _customerRepositoryMock
            .Setup(r => r.GetAllByTenantAsync(tenantId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Customer>());

        var query = new GetAllCustomersQuery();

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

        var filters = new CustomerFilters
        {
            SearchTerm = "Test",
            IsActive = true
        };

        var customers = new List<Customer>
        {
            new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = "Test Customer",
                Email = "test@example.com",
                IsActive = true
            }
        };

        _customerRepositoryMock
            .Setup(r => r.GetAllByTenantAsync(tenantId, filters, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customers);

        var query = new GetAllCustomersQuery { Filters = filters };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(1);

        _customerRepositoryMock.Verify(r => r.GetAllByTenantAsync(tenantId, filters, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var query = new GetAllCustomersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");
    }

    [Fact]
    public async Task Handle_CustomersWithoutCountry_ShouldReturnNullCountryNames()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var customers = new List<Customer>
        {
            new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = "Customer Without Country",
                Email = "nocountry@example.com",
                BillingCountry = null,
                ShippingCountry = null,
                IsActive = true
            }
        };

        _customerRepositoryMock
            .Setup(r => r.GetAllByTenantAsync(tenantId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customers);

        var query = new GetAllCustomersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value![0].BillingCountryName.Should().BeNull();
        result.Value![0].ShippingCountryName.Should().BeNull();
    }
}
