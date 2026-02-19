using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.Customers.Commands.UpdateCustomer;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.Customers.Commands;

public class UpdateCustomerCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<UpdateCustomerCommandHandler>> _loggerMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly UpdateCustomerCommandHandler _handler;

    public UpdateCustomerCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<UpdateCustomerCommandHandler>>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();

        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepositoryMock.Object);

        _handler = new UpdateCustomerCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateCustomer()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var existingCustomer = new Customer
        {
            Id = customerId,
            TenantId = tenantId,
            Name = "Old Name",
            Email = "old@example.com",
            TaxId = "OLD123",
            IsActive = true
        };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        _customerRepositoryMock
            .Setup(r => r.GetByEmailAsync("new@example.com", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        _customerRepositoryMock
            .Setup(r => r.GetByTaxIdAsync("NEW456", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var command = new UpdateCustomerCommand
        {
            Id = customerId,
            Name = "Updated Name",
            Email = "new@example.com",
            Phone = "+9876543210",
            TaxId = "NEW456",
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Updated Name");
        result.Value.Email.Should().Be("new@example.com");
        result.Value.TaxId.Should().Be("NEW456");

        _customerRepositoryMock.Verify(r => r.UpdateAsync(
            It.Is<Customer>(c =>
                c.Id == customerId &&
                c.Name == "Updated Name" &&
                c.Email == "new@example.com"),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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

        var command = new UpdateCustomerCommand
        {
            Id = customerId,
            Name = "Updated Name",
            Email = "test@example.com",
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");

        _customerRepositoryMock.Verify(r => r.UpdateAsync(
            It.IsAny<Customer>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WrongTenant_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var differentTenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var existingCustomer = new Customer
        {
            Id = customerId,
            TenantId = differentTenantId,
            Name = "Customer",
            Email = "test@example.com"
        };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        var command = new UpdateCustomerCommand
        {
            Id = customerId,
            Name = "Updated Name",
            Email = "test@example.com",
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Unauthorized");
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var otherCustomerId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var existingCustomer = new Customer
        {
            Id = customerId,
            TenantId = tenantId,
            Name = "Customer",
            Email = "old@example.com"
        };

        var customerWithSameEmail = new Customer
        {
            Id = otherCustomerId,
            TenantId = tenantId,
            Name = "Other Customer",
            Email = "duplicate@example.com"
        };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        _customerRepositoryMock
            .Setup(r => r.GetByEmailAsync("duplicate@example.com", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customerWithSameEmail);

        var command = new UpdateCustomerCommand
        {
            Id = customerId,
            Name = "Updated Name",
            Email = "duplicate@example.com",
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");
        result.Error.Should().Contain("duplicate@example.com");
    }

    [Fact]
    public async Task Handle_DuplicateTaxId_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var otherCustomerId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var existingCustomer = new Customer
        {
            Id = customerId,
            TenantId = tenantId,
            Name = "Customer",
            Email = "test@example.com",
            TaxId = "OLD123"
        };

        var customerWithSameTaxId = new Customer
        {
            Id = otherCustomerId,
            TenantId = tenantId,
            Name = "Other Customer",
            Email = "other@example.com",
            TaxId = "DUPLICATE456"
        };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        _customerRepositoryMock
            .Setup(r => r.GetByEmailAsync("test@example.com", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        _customerRepositoryMock
            .Setup(r => r.GetByTaxIdAsync("DUPLICATE456", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customerWithSameTaxId);

        var command = new UpdateCustomerCommand
        {
            Id = customerId,
            Name = "Updated Name",
            Email = "test@example.com",
            TaxId = "DUPLICATE456",
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");
        result.Error.Should().Contain("DUPLICATE456");
    }

    [Fact]
    public async Task Handle_SameEmailAsOwn_ShouldUpdateSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var existingCustomer = new Customer
        {
            Id = customerId,
            TenantId = tenantId,
            Name = "Old Name",
            Email = "same@example.com"
        };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        var command = new UpdateCustomerCommand
        {
            Id = customerId,
            Name = "Updated Name",
            Email = "same@example.com",
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Should not check for duplicate email when it's the same
        _customerRepositoryMock.Verify(r => r.GetByEmailAsync(
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new UpdateCustomerCommand
        {
            Id = Guid.NewGuid(),
            Name = "Customer",
            Email = "test@example.com",
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");
    }
}
