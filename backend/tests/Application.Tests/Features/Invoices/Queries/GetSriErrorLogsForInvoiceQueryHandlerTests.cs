using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.DTOs;
using SaaS.Application.Features.Invoices.Queries.GetSriErrorLogs;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;
using Xunit;

namespace Application.Tests.Features.Invoices.Queries;

public class GetSriErrorLogsForInvoiceQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetSriErrorLogsForInvoiceQueryHandler>> _loggerMock;
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
    private readonly Mock<ISriErrorLogRepository> _sriErrorLogRepositoryMock;
    private readonly GetSriErrorLogsForInvoiceQueryHandler _handler;

    public GetSriErrorLogsForInvoiceQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetSriErrorLogsForInvoiceQueryHandler>>();
        _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
        _sriErrorLogRepositoryMock = new Mock<ISriErrorLogRepository>();

        _unitOfWorkMock.Setup(u => u.Invoices).Returns(_invoiceRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.SriErrorLogs).Returns(_sriErrorLogRepositoryMock.Object);

        _handler = new GetSriErrorLogsForInvoiceQueryHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    // -----------------------------------------------------------------------
    // Invoice lookup failures
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_InvoiceNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _invoiceRepositoryMock
            .Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice?)null);

        var query = new GetSriErrorLogsForInvoiceQuery(invoiceId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invoice not found");

        _sriErrorLogRepositoryMock.Verify(
            r => r.GetByInvoiceIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_InvoiceFromDifferentTenant_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var invoice = new Invoice { Id = invoiceId, TenantId = otherTenantId };
        _invoiceRepositoryMock
            .Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var query = new GetSriErrorLogsForInvoiceQuery(invoiceId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invoice not found");
    }

    // -----------------------------------------------------------------------
    // Success — invoice with no logs
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_InvoiceWithNoLogs_ShouldReturnEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var invoice = new Invoice { Id = invoiceId, TenantId = tenantId };
        _invoiceRepositoryMock
            .Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        _sriErrorLogRepositoryMock
            .Setup(r => r.GetByInvoiceIdAsync(invoiceId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SriErrorLog>());

        var query = new GetSriErrorLogsForInvoiceQuery(invoiceId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull().And.BeEmpty();
    }

    // -----------------------------------------------------------------------
    // Success — logs returned ordered by OccurredAt descending
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_MultipleLogsExist_ShouldReturnOrderedByOccurredAtDescending()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var invoice = new Invoice { Id = invoiceId, TenantId = tenantId };
        _invoiceRepositoryMock
            .Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        // Return logs out of order to verify the handler sorts them
        var logs = new List<SriErrorLog>
        {
            new SriErrorLog
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoiceId,
                TenantId = tenantId,
                Operation = "SubmitToSri",
                ErrorCode = "001",
                ErrorMessage = "First error",
                OccurredAt = now.AddMinutes(-30)
            },
            new SriErrorLog
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoiceId,
                TenantId = tenantId,
                Operation = "CheckAuthorization",
                ErrorCode = "035",
                ErrorMessage = "Latest error",
                OccurredAt = now
            },
            new SriErrorLog
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoiceId,
                TenantId = tenantId,
                Operation = "SubmitToSri",
                ErrorCode = "002",
                ErrorMessage = "Middle error",
                OccurredAt = now.AddMinutes(-15)
            }
        };

        _sriErrorLogRepositoryMock
            .Setup(r => r.GetByInvoiceIdAsync(invoiceId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);

        var query = new GetSriErrorLogsForInvoiceQuery(invoiceId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);

        // First item should be the most recent
        result.Value![0].OccurredAt.Should().Be(now);
        result.Value[0].ErrorCode.Should().Be("035");
        result.Value[0].ErrorMessage.Should().Be("Latest error");

        result.Value[1].OccurredAt.Should().Be(now.AddMinutes(-15));
        result.Value[2].OccurredAt.Should().Be(now.AddMinutes(-30));
    }

    // -----------------------------------------------------------------------
    // DTO mapping
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_SingleLog_ShouldMapFieldsCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var logId = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow.AddHours(-1);

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var invoice = new Invoice { Id = invoiceId, TenantId = tenantId };
        _invoiceRepositoryMock
            .Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var log = new SriErrorLog
        {
            Id = logId,
            InvoiceId = invoiceId,
            TenantId = tenantId,
            Operation = "CheckAuthorization",
            ErrorCode = "043",
            ErrorMessage = "RUC del emisor no existe",
            AdditionalData = "{\"detail\": \"extra info\"}",
            OccurredAt = occurredAt
        };

        _sriErrorLogRepositoryMock
            .Setup(r => r.GetByInvoiceIdAsync(invoiceId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SriErrorLog> { log });

        var query = new GetSriErrorLogsForInvoiceQuery(invoiceId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);

        var dto = result.Value![0];
        dto.Id.Should().Be(logId);
        dto.Operation.Should().Be("CheckAuthorization");
        dto.ErrorCode.Should().Be("043");
        dto.ErrorMessage.Should().Be("RUC del emisor no existe");
        dto.AdditionalData.Should().Be("{\"detail\": \"extra info\"}");
        dto.OccurredAt.Should().Be(occurredAt);
    }
}
