using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.Payments.Queries.GetPaymentById;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace Application.Tests.Features.Payments.Queries;

public class GetPaymentByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetPaymentByIdQueryHandler>> _loggerMock;
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly GetPaymentByIdQueryHandler _handler;

    public GetPaymentByIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetPaymentByIdQueryHandler>>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();

        _unitOfWorkMock.Setup(u => u.Payments).Returns(_paymentRepositoryMock.Object);

        _handler = new GetPaymentByIdQueryHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingPayment_ShouldReturnPaymentDto()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var payment = new Payment
        {
            Id = paymentId,
            TenantId = tenantId,
            InvoiceId = Guid.NewGuid(),
            Amount = 500.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = SriPaymentMethod.Cash,
            Status = PaymentStatus.Completed,
            IsDeleted = false
        };

        _paymentRepositoryMock
            .Setup(r => r.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var query = new GetPaymentByIdQuery(paymentId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(paymentId);
        result.Value.Amount.Should().Be(500.00m);
        result.Value.Status.Should().Be(PaymentStatus.Completed);
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

        var query = new GetPaymentByIdQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Payment not found");
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var query = new GetPaymentByIdQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant");
    }
}
