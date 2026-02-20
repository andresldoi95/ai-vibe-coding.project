using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.Payments.Commands.CreatePayment;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.DTOs;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace Application.Tests.Features.Payments.Commands;

public class CreatePaymentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<CreatePaymentCommandHandler>> _loggerMock;
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly CreatePaymentCommandHandler _handler;

    public CreatePaymentCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<CreatePaymentCommandHandler>>();
        _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();

        _unitOfWorkMock.Setup(u => u.Invoices).Returns(_invoiceRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Payments).Returns(_paymentRepositoryMock.Object);

        _handler = new CreatePaymentCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidPayment_ShouldCreateSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var invoice = new Invoice
        {
            Id = invoiceId,
            TenantId = tenantId,
            TotalAmount = 1000.00m,
            Status = InvoiceStatus.Sent,
            IsDeleted = false
        };

        _invoiceRepositoryMock
            .Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        _paymentRepositoryMock
            .Setup(r => r.GetByInvoiceIdAsync(invoiceId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Payment>());

        // Mock the payment retrieval after creation
        _paymentRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken ct) => new Payment
            {
                Id = id,
                TenantId = tenantId,
                InvoiceId = invoiceId,
                Amount = 500.00m,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = SriPaymentMethod.Cash,
                Status = PaymentStatus.Completed
            });

        var command = new CreatePaymentCommand
        {
            InvoiceId = invoiceId,
            Amount = 500.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = SriPaymentMethod.Cash,
            Status = PaymentStatus.Completed
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Amount.Should().Be(500.00m);

        _paymentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FullPayment_ShouldMarkInvoiceAsPaid()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var invoice = new Invoice
        {
            Id = invoiceId,
            TenantId = tenantId,
            TotalAmount = 1000.00m,
            Status = InvoiceStatus.Sent,
            IsDeleted = false
        };

        _invoiceRepositoryMock
            .Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        _paymentRepositoryMock
            .Setup(r => r.GetByInvoiceIdAsync(invoiceId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Payment>());

        // Mock the payment retrieval after creation
        _paymentRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken ct) => new Payment
            {
                Id = id,
                TenantId = tenantId,
                InvoiceId = invoiceId,
                Amount = 1000.00m,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = SriPaymentMethod.Cash,
                Status = PaymentStatus.Completed
            });

        var command = new CreatePaymentCommand
        {
            InvoiceId = invoiceId,
            Amount = 1000.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = SriPaymentMethod.Cash,
            Status = PaymentStatus.Completed
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        invoice.Status.Should().Be(InvoiceStatus.Paid);
        invoice.PaidDate.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_AmountExceedsRemainingBalance_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var invoice = new Invoice
        {
            Id = invoiceId,
            TenantId = tenantId,
            TotalAmount = 1000.00m,
            Status = InvoiceStatus.Sent,
            IsDeleted = false
        };

        var existingPayment = new Payment
        {
            InvoiceId = invoiceId,
            Amount = 600.00m,
            Status = PaymentStatus.Completed
        };

        _invoiceRepositoryMock
            .Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        _paymentRepositoryMock
            .Setup(r => r.GetByInvoiceIdAsync(invoiceId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Payment> { existingPayment });

        var command = new CreatePaymentCommand
        {
            InvoiceId = invoiceId,
            Amount = 500.00m, // Exceeds remaining 400.00
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = SriPaymentMethod.Cash,
            Status = PaymentStatus.Completed
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("remaining balance");

        _paymentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InvoiceNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _invoiceRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice?)null);

        var command = new CreatePaymentCommand
        {
            InvoiceId = Guid.NewGuid(),
            Amount = 100.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = SriPaymentMethod.Cash
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invoice not found");
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new CreatePaymentCommand
        {
            InvoiceId = Guid.NewGuid(),
            Amount = 100.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = SriPaymentMethod.Cash
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant");
    }
}
