using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.DTOs.Sri;
using SaaS.Application.Features.CreditNotes.Commands.SubmitCreditNoteToSri;
using SaaS.Application.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;
using Xunit;

namespace Application.Tests.Features.CreditNotes.Commands;

public class SubmitCreditNoteToSriCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ISriWebServiceClient> _sriWebServiceClientMock;
    private readonly Mock<ILogger<SubmitCreditNoteToSriCommandHandler>> _loggerMock;
    private readonly Mock<ICreditNoteRepository> _creditNoteRepositoryMock;
    private readonly Mock<ISriErrorLogRepository> _sriErrorLogRepositoryMock;
    private readonly SubmitCreditNoteToSriCommandHandler _handler;

    public SubmitCreditNoteToSriCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _sriWebServiceClientMock = new Mock<ISriWebServiceClient>();
        _loggerMock = new Mock<ILogger<SubmitCreditNoteToSriCommandHandler>>();
        _creditNoteRepositoryMock = new Mock<ICreditNoteRepository>();
        _sriErrorLogRepositoryMock = new Mock<ISriErrorLogRepository>();

        _unitOfWorkMock.Setup(u => u.CreditNotes).Returns(_creditNoteRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.SriErrorLogs).Returns(_sriErrorLogRepositoryMock.Object);

        _handler = new SubmitCreditNoteToSriCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _sriWebServiceClientMock.Object,
            _loggerMock.Object);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidCreditNote_ShouldSubmitAndReturnSuccess()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        var signedXmlPath = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(signedXmlPath, "<signedXml>test</signedXml>");

            _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

            var creditNote = new CreditNote
            {
                Id = creditNoteId,
                TenantId = tenantId,
                Status = InvoiceStatus.PendingAuthorization,
                SignedXmlFilePath = signedXmlPath
            };

            _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);
            _sriWebServiceClientMock.Setup(c => c.SubmitDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SriSubmissionResponse { IsSuccess = true, Message = "RECIBIDA" });

            var command = new SubmitCreditNoteToSriCommand { CreditNoteId = creditNoteId };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Contain("RECIBIDA");

            // Status stays PendingAuthorization — SRI just acknowledged receipt
            creditNote.Status.Should().Be(InvoiceStatus.PendingAuthorization);

            _creditNoteRepositoryMock.Verify(r => r.UpdateAsync(creditNote, It.IsAny<CancellationToken>()), Times.Once);
            _sriWebServiceClientMock.Verify(c => c.SubmitDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        finally { File.Delete(signedXmlPath); }
    }

    // ── Credit note not found ─────────────────────────────────────────────────

    [Fact]
    public async Task Handle_CreditNoteNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((CreditNote?)null);

        var command = new SubmitCreditNoteToSriCommand { CreditNoteId = Guid.NewGuid() };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Credit note not found");
        _sriWebServiceClientMock.Verify(c => c.SubmitDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── Status guard ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(InvoiceStatus.Draft)]
    [InlineData(InvoiceStatus.PendingSignature)]
    [InlineData(InvoiceStatus.Authorized)]
    public async Task Handle_WrongStatus_ShouldReturnFailure(InvoiceStatus status)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = status };
        _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);

        var command = new SubmitCreditNoteToSriCommand { CreditNoteId = creditNoteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _sriWebServiceClientMock.Verify(c => c.SubmitDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RejectedCreditNote_ShouldReturnRejectionReasons()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = InvoiceStatus.Rejected };
        _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);
        _sriErrorLogRepositoryMock.Setup(r => r.GetByCreditNoteIdAsync(creditNoteId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SriErrorLog>
            {
                new() { ErrorCode = "REJECT_001", ErrorMessage = "Campo inválido", TenantId = tenantId }
            });

        var command = new SubmitCreditNoteToSriCommand { CreditNoteId = creditNoteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("rejected by SRI");
        result.Error.Should().Contain("REJECT_001");
    }

    // ── Signed XML file missing ───────────────────────────────────────────────

    [Fact]
    public async Task Handle_SignedXmlPathNull_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = InvoiceStatus.PendingAuthorization, SignedXmlFilePath = null };
        _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);

        var command = new SubmitCreditNoteToSriCommand { CreditNoteId = creditNoteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Signed XML file not found");
    }

    [Fact]
    public async Task Handle_SignedXmlFileNotOnDisk_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = InvoiceStatus.PendingAuthorization, SignedXmlFilePath = "/nonexistent/cn-signed.xml" };
        _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);

        var command = new SubmitCreditNoteToSriCommand { CreditNoteId = creditNoteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found on disk");
    }

    // ── SRI returns failure ───────────────────────────────────────────────────

    [Fact]
    public async Task Handle_SriReturnsFailure_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        var signedXmlPath = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(signedXmlPath, "<signedXml/>");
            _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

            var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = InvoiceStatus.PendingAuthorization, SignedXmlFilePath = signedXmlPath };
            _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);
            _sriWebServiceClientMock.Setup(c => c.SubmitDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SriSubmissionResponse
                {
                    IsSuccess = false,
                    Message = "DEVUELTA",
                    Errors = new List<SriError> { new() { Code = "ERR_001", Message = "RUC inválido" } }
                });

            var command = new SubmitCreditNoteToSriCommand { CreditNoteId = creditNoteId };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("SRI submission failed");
        }
        finally { File.Delete(signedXmlPath); }
    }

    // ── SOAP exception ────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_SoapException_ShouldLogSriErrorAndReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        var signedXmlPath = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(signedXmlPath, "<signedXml/>");
            _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

            var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = InvoiceStatus.PendingAuthorization, SignedXmlFilePath = signedXmlPath };
            _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);
            _sriWebServiceClientMock.Setup(c => c.SubmitDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("SOAP connection failed"));

            var command = new SubmitCreditNoteToSriCommand { CreditNoteId = creditNoteId };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("error occurred");

            _sriErrorLogRepositoryMock.Verify(
                r => r.AddAsync(It.Is<SriErrorLog>(e => e.ErrorCode == "SOAP_ERROR" && e.CreditNoteId == creditNoteId), It.IsAny<CancellationToken>()),
                Times.Once);
        }
        finally { File.Delete(signedXmlPath); }
    }

    // ── No tenant ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new SubmitCreditNoteToSriCommand { CreditNoteId = Guid.NewGuid() };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
