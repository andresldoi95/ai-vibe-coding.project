using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.Customers.Commands.CreateCustomer;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.Customers.Commands;

public class CreateCustomerCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<CreateCustomerCommandHandler>> _loggerMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly CreateCustomerCommandHandler _handler;

    public CreateCustomerCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<CreateCustomerCommandHandler>>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();

        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepositoryMock.Object);

        _handler = new CreateCustomerCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateCustomer()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _customerRepositoryMock
            .Setup(r => r.GetByEmailAsync("test@example.com", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        _customerRepositoryMock
            .Setup(r => r.GetByTaxIdAsync("1234567890", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var command = new CreateCustomerCommand
        {
            Name = "Test Customer",
            Email = "test@example.com",
            Phone = "+1234567890",
            TaxId = "1234567890",
            ContactPerson = "John Doe",
            BillingStreet = "123 Main St",
            BillingCity = "New York",
            BillingState = "NY",
            BillingPostalCode = "10001",
            BillingCountryId = Guid.NewGuid(),
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Test Customer");
        result.Value.Email.Should().Be("test@example.com");
        result.Value.TaxId.Should().Be("1234567890");

        _customerRepositoryMock.Verify(r => r.AddAsync(
            It.Is<Customer>(c =>
                c.Name == "Test Customer" &&
                c.Email == "test@example.com" &&
                c.TenantId == tenantId),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var existingCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "Existing Customer",
            Email = "test@example.com"
        };

        _customerRepositoryMock
            .Setup(r => r.GetByEmailAsync("test@example.com", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        var command = new CreateCustomerCommand
        {
            Name = "New Customer",
            Email = "test@example.com",
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");
        result.Error.Should().Contain("test@example.com");

        _customerRepositoryMock.Verify(r => r.AddAsync(
            It.IsAny<Customer>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicateTaxId_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _customerRepositoryMock
            .Setup(r => r.GetByEmailAsync("test@example.com", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var existingCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "Existing Customer",
            Email = "existing@example.com",
            TaxId = "1234567890"
        };

        _customerRepositoryMock
            .Setup(r => r.GetByTaxIdAsync("1234567890", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        var command = new CreateCustomerCommand
        {
            Name = "New Customer",
            Email = "test@example.com",
            TaxId = "1234567890",
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");
        result.Error.Should().Contain("1234567890");

        _customerRepositoryMock.Verify(r => r.AddAsync(
            It.IsAny<Customer>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new CreateCustomerCommand
        {
            Name = "Test Customer",
            Email = "test@example.com",
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");

        _customerRepositoryMock.Verify(r => r.AddAsync(
            It.IsAny<Customer>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithoutTaxId_ShouldCreateCustomerSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _customerRepositoryMock
            .Setup(r => r.GetByEmailAsync("test@example.com", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var command = new CreateCustomerCommand
        {
            Name = "Test Customer",
            Email = "test@example.com",
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TaxId.Should().BeNull();

        _customerRepositoryMock.Verify(r => r.GetByTaxIdAsync(
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithCompleteAddress_ShouldCreateCustomerWithAllFields()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var billingCountryId = Guid.NewGuid();
        var shippingCountryId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _customerRepositoryMock
            .Setup(r => r.GetByEmailAsync("test@example.com", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var command = new CreateCustomerCommand
        {
            Name = "Complete Customer",
            Email = "test@example.com",
            Phone = "+1234567890",
            TaxId = "1234567890",
            ContactPerson = "Jane Doe",
            BillingStreet = "123 Billing St",
            BillingCity = "Billing City",
            BillingState = "BC",
            BillingPostalCode = "12345",
            BillingCountryId = billingCountryId,
            ShippingStreet = "456 Shipping Ave",
            ShippingCity = "Shipping City",
            ShippingState = "SC",
            ShippingPostalCode = "67890",
            ShippingCountryId = shippingCountryId,
            Notes = "Important customer notes",
            Website = "https://example.com",
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.BillingCity.Should().Be("Billing City");
        result.Value.ShippingCity.Should().Be("Shipping City");
        result.Value.Notes.Should().Be("Important customer notes");
        result.Value.Website.Should().Be("https://example.com");
    }
}
