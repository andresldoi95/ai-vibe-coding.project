using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.Invoices.Commands.ChangeInvoiceStatus;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace Application.Tests.Features.Invoices.Commands;

public class ChangeInvoiceStatusCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<ChangeInvoiceStatusCommandHandler>> _loggerMock;
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
    private readonly ChangeInvoiceStatusCommandHandler _handler;

    public ChangeInvoiceStatusCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<ChangeInvoiceStatusCommandHandler>>();
        _invoiceRepositoryMock = new Mock<IInvoiceRepository>();

        _unitOfWorkMock.Setup(u => u.Invoices).Returns(_invoiceRepositoryMock.Object);

        _handler = new ChangeInvoiceStatusCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Theory]
    [InlineData(InvoiceStatus.Draft, InvoiceStatus.Sent)]
    [InlineData(InvoiceStatus.Draft, InvoiceStatus.Cancelled)]
    [InlineData(InvoiceStatus.Sent, InvoiceStatus.Paid)]
    [InlineData(InvoiceStatus.Sent, InvoiceStatus.Overdue)]
    [InlineData(InvoiceStatus.Sent, InvoiceStatus.Cancelled)]
    [InlineData(InvoiceStatus.Overdue, InvoiceStatus.Paid)]
    [InlineData(InvoiceStatus.Overdue, InvoiceStatus.Cancelled)]
    public async Task Handle_ValidStatusTransition_ShouldUpdateStatus(
        InvoiceStatus currentStatus, InvoiceStatus newStatus)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var invoice = new Invoice
        {
            Id = invoiceId,
            TenantId = tenantId,
            InvoiceNumber = "001-001-000000001",
            Status = currentStatus,
            TotalAmount = 100.00m,
            Items = new List<InvoiceItem>()
        };

        _invoiceRepositoryMock
            .Setup(r => r.GetWithItemsAsync(invoiceId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var command = new ChangeInvoiceStatusCommand
        {
            Id = invoiceId,
            NewStatus = newStatus
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Status.Should().Be(newStatus);
        invoice.Status.Should().Be(newStatus);

        _invoiceRepositoryMock.Verify(r => r.UpdateAsync(invoice, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(InvoiceStatus.Draft, InvoiceStatus.Paid)]
    [InlineData(InvoiceStatus.Draft, InvoiceStatus.Overdue)]
    [InlineData(InvoiceStatus.Sent, InvoiceStatus.Draft)]
    [InlineData(InvoiceStatus.Paid, InvoiceStatus.Draft)]
    [InlineData(InvoiceStatus.Paid, InvoiceStatus.Sent)]
    [InlineData(InvoiceStatus.Paid, InvoiceStatus.Cancelled)]
    [InlineData(InvoiceStatus.Cancelled, InvoiceStatus.Draft)]
    [InlineData(InvoiceStatus.Cancelled, InvoiceStatus.Sent)]
    [InlineData(InvoiceStatus.Cancelled, InvoiceStatus.Paid)]
    public async Task Handle_InvalidStatusTransition_ShouldReturnFailure(
        InvoiceStatus currentStatus, InvoiceStatus newStatus)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var invoice = new Invoice
        {
            Id = invoiceId,
            TenantId = tenantId,
            InvoiceNumber = "001-001-000000001",
            Status = currentStatus,
            TotalAmount = 100.00m,
            Items = new List<InvoiceItem>()
        };

        _invoiceRepositoryMock
            .Setup(r => r.GetWithItemsAsync(invoiceId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var command = new ChangeInvoiceStatusCommand
        {
            Id = invoiceId,
            NewStatus = newStatus
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Cannot change status from");

        _invoiceRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_StatusChangeToPaid_ShouldSetPaidDate()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var invoice = new Invoice
        {
            Id = invoiceId,
            TenantId = tenantId,
            InvoiceNumber = "001-001-000000001",
            Status = InvoiceStatus.Sent,
            TotalAmount = 100.00m,
            PaidDate = null,
            Items = new List<InvoiceItem>()
        };

        _invoiceRepositoryMock
            .Setup(r => r.GetWithItemsAsync(invoiceId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var command = new ChangeInvoiceStatusCommand
        {
            Id = invoiceId,
            NewStatus = InvoiceStatus.Paid
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        invoice.PaidDate.Should().NotBeNull();
        invoice.PaidDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_InvoiceNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _invoiceRepositoryMock
            .Setup(r => r.GetWithItemsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice?)null);

        var command = new ChangeInvoiceStatusCommand
        {
            Id = Guid.NewGuid(),
            NewStatus = InvoiceStatus.Sent
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

        var command = new ChangeInvoiceStatusCommand
        {
            Id = Guid.NewGuid(),
            NewStatus = InvoiceStatus.Sent
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }
}
