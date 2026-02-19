using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.Payments.Commands.VoidPayment;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace Application.Tests.Features.Payments.Commands;

public class VoidPaymentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<VoidPaymentCommandHandler>> _loggerMock;
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
    private readonly VoidPaymentCommandHandler _handler;

    public VoidPaymentCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<VoidPaymentCommandHandler>>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _invoiceRepositoryMock = new Mock<IInvoiceRepository>();

        _unitOfWorkMock.Setup(u => u.Payments).Returns(_paymentRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Invoices).Returns(_invoiceRepositoryMock.Object);

        _handler = new VoidPaymentCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_CompletedPayment_ShouldVoidSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var payment = new Payment
        {
            Id = paymentId,
            TenantId = tenantId,
            InvoiceId = invoiceId,
            Amount = 500.00m,
            Status = PaymentStatus.Completed,
            IsDeleted = false
        };

        var invoice = new Invoice
        {
            Id = invoiceId,
            TenantId = tenantId,
            TotalAmount = 1000.00m,
            Status = InvoiceStatus.Paid
        };

        _paymentRepositoryMock
            .Setup(r => r.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _invoiceRepositoryMock
            .Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        _paymentRepositoryMock
            .Setup(r => r.GetByInvoiceIdAsync(invoiceId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Payment> { payment });

        var command = new VoidPaymentCommand
        {
            Id = paymentId,
            Reason = "Duplicate payment"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Voided);
        payment.Notes.Should().Contain("Voided: Duplicate payment");

        _paymentRepositoryMock.Verify(r => r.UpdateAsync(payment, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AlreadyVoided_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var payment = new Payment
        {
            Id = paymentId,
            TenantId = tenantId,
            Status = PaymentStatus.Voided,
            IsDeleted = false
        };

        _paymentRepositoryMock
            .Setup(r => r.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var command = new VoidPaymentCommand
        {
            Id = paymentId,
            Reason = "Test"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already voided");

        _paymentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PaymentNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _paymentRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var command = new VoidPaymentCommand
        {
            Id = Guid.NewGuid(),
            Reason = "Test"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Payment not found");
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new VoidPaymentCommand
        {
            Id = Guid.NewGuid(),
            Reason = "Test"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant");
    }
}
