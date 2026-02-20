using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.Payments.Queries.GetAllPayments;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace Application.Tests.Features.Payments.Queries;

public class GetAllPaymentsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetAllPaymentsQueryHandler>> _loggerMock;
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
    private readonly GetAllPaymentsQueryHandler _handler;

    public GetAllPaymentsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetAllPaymentsQueryHandler>>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _invoiceRepositoryMock = new Mock<IInvoiceRepository>();

        _unitOfWorkMock.Setup(u => u.Payments).Returns(_paymentRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Invoices).Returns(_invoiceRepositoryMock.Object);

        _handler = new GetAllPaymentsQueryHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnAllPayments()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var customer = new Customer
        {
            Id = customerId,
            TenantId = tenantId,
            Name = "Test Customer"
        };

        var invoice = new Invoice
        {
            Id = invoiceId,
            TenantId = tenantId,
            CustomerId = customerId,
            Customer = customer,
            InvoiceNumber = "INV-001",
            TotalAmount = 1000.00m
        };

        var payments = new List<Payment>
        {
            new Payment
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                InvoiceId = invoiceId,
                Invoice = invoice,
                Amount = 500.00m,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = SriPaymentMethod.Cash,
                Status = PaymentStatus.Completed
            },
            new Payment
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                InvoiceId = invoiceId,
                Invoice = invoice,
                Amount = 500.00m,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = SriPaymentMethod.BankTransfer,
                Status = PaymentStatus.Pending
            }
        };

        _paymentRepositoryMock
            .Setup(r => r.GetAllByTenantAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payments);

        var query = new GetAllPaymentsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value![0].Amount.Should().Be(500.00m);
        result.Value[0].InvoiceNumber.Should().Be("INV-001");
        result.Value[0].CustomerName.Should().Be("Test Customer");
    }

    [Fact]
    public async Task Handle_NoPayments_ShouldReturnEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _paymentRepositoryMock
            .Setup(r => r.GetAllByTenantAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Payment>());

        var query = new GetAllPaymentsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var query = new GetAllPaymentsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant");
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldIncludePaymentDetails()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var customer = new Customer
        {
            Id = customerId,
            TenantId = tenantId,
            Name = "Acme Corp"
        };

        var invoice = new Invoice
        {
            Id = invoiceId,
            TenantId = tenantId,
            CustomerId = customerId,
            Customer = customer,
            InvoiceNumber = "INV-2024-001",
            TotalAmount = 2500.00m
        };

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            InvoiceId = invoiceId,
            Invoice = invoice,
            Amount = 2500.00m,
            PaymentDate = new DateTime(2024, 1, 15),
            PaymentMethod = SriPaymentMethod.CreditCard,
            Status = PaymentStatus.Completed,
            TransactionId = "TXN123456",
            Notes = "Full payment received"
        };

        _paymentRepositoryMock
            .Setup(r => r.GetAllByTenantAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Payment> { payment });

        var query = new GetAllPaymentsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);

        var paymentDto = result.Value![0];
        paymentDto.Id.Should().Be(payment.Id);
        paymentDto.Amount.Should().Be(2500.00m);
        paymentDto.InvoiceNumber.Should().Be("INV-2024-001");
        paymentDto.CustomerName.Should().Be("Acme Corp");
        paymentDto.PaymentMethod.Should().Be(SriPaymentMethod.CreditCard);
        paymentDto.Status.Should().Be(PaymentStatus.Completed);
        paymentDto.TransactionId.Should().Be("TXN123456");
        paymentDto.Notes.Should().Be("Full payment received");
    }
}
