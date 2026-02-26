using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Features.CreditNotes.Queries.GetSriErrorLogsForCreditNote;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;
using Xunit;

namespace Application.Tests.Features.CreditNotes.Queries;

public class GetSriErrorLogsForCreditNoteQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetSriErrorLogsForCreditNoteQueryHandler>> _loggerMock;
    private readonly Mock<ICreditNoteRepository> _creditNoteRepositoryMock;
    private readonly Mock<ISriErrorLogRepository> _sriErrorLogRepositoryMock;
    private readonly GetSriErrorLogsForCreditNoteQueryHandler _handler;

    public GetSriErrorLogsForCreditNoteQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetSriErrorLogsForCreditNoteQueryHandler>>();
        _creditNoteRepositoryMock = new Mock<ICreditNoteRepository>();
        _sriErrorLogRepositoryMock = new Mock<ISriErrorLogRepository>();

        _unitOfWorkMock.Setup(u => u.CreditNotes).Returns(_creditNoteRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.SriErrorLogs).Returns(_sriErrorLogRepositoryMock.Object);

        _handler = new GetSriErrorLogsForCreditNoteQueryHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ExistingCreditNoteWithErrors_ShouldReturnMappedDtos()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = InvoiceStatus.Draft };
        _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);

        var occurredAt = new DateTime(2026, 2, 25, 10, 0, 0, DateTimeKind.Utc);
        var errorLog = new SriErrorLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CreditNoteId = creditNoteId,
            Operation = "Submit",
            ErrorCode = "SOAP_ERROR",
            ErrorMessage = "Connection refused",
            AdditionalData = "<fault/>",
            OccurredAt = occurredAt
        };
        _sriErrorLogRepositoryMock.Setup(r => r.GetByCreditNoteIdAsync(creditNoteId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SriErrorLog> { errorLog });

        var query = new GetSriErrorLogsForCreditNoteQuery(creditNoteId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);

        var dto = result.Value.First();
        dto.Id.Should().Be(errorLog.Id);
        dto.Operation.Should().Be("Submit");
        dto.ErrorCode.Should().Be("SOAP_ERROR");
        dto.ErrorMessage.Should().Be("Connection refused");
        dto.AdditionalData.Should().Be("<fault/>");
        dto.OccurredAt.Should().Be(occurredAt);
    }

    [Fact]
    public async Task Handle_ExistingCreditNoteWithNoErrors_ShouldReturnSuccessWithEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = InvoiceStatus.Draft };
        _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);
        _sriErrorLogRepositoryMock.Setup(r => r.GetByCreditNoteIdAsync(creditNoteId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SriErrorLog>());

        // Act
        var result = await _handler.Handle(new GetSriErrorLogsForCreditNoteQuery(creditNoteId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // ── Not found ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_CreditNoteNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((CreditNote?)null);

        // Act
        var result = await _handler.Handle(new GetSriErrorLogsForCreditNoteQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Credit note not found");
        _sriErrorLogRepositoryMock.Verify(r => r.GetByCreditNoteIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CreditNoteFromDifferentTenant_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreditNote { Id = creditNoteId, TenantId = otherTenantId });

        // Act
        var result = await _handler.Handle(new GetSriErrorLogsForCreditNoteQuery(creditNoteId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Credit note not found");
        _sriErrorLogRepositoryMock.Verify(r => r.GetByCreditNoteIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── No tenant ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_NoTenantContext_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        // Act / Assert — this handler throws (does not return Result.Failure) when no tenant
        await FluentActions
            .Invoking(() => _handler.Handle(new GetSriErrorLogsForCreditNoteQuery(Guid.NewGuid()), CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>();
        _sriErrorLogRepositoryMock.Verify(r => r.GetByCreditNoteIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
