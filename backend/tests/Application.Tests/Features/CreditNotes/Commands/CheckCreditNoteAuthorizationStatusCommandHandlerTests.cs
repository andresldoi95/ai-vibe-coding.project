using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.DTOs.Sri;
using SaaS.Application.Features.CreditNotes.Commands.CheckCreditNoteAuthorizationStatus;
using SaaS.Application.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;
using Xunit;

namespace Application.Tests.Features.CreditNotes.Commands;

public class CheckCreditNoteAuthorizationStatusCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ISriWebServiceClient> _sriWebServiceClientMock;
    private readonly Mock<ILogger<CheckCreditNoteAuthorizationStatusCommandHandler>> _loggerMock;
    private readonly Mock<ICreditNoteRepository> _creditNoteRepositoryMock;
    private readonly Mock<ISriErrorLogRepository> _sriErrorLogRepositoryMock;
    private readonly CheckCreditNoteAuthorizationStatusCommandHandler _handler;

    private const string ValidAccessKey = "4902202504019990000000001100100100000000122345671";

    public CheckCreditNoteAuthorizationStatusCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _sriWebServiceClientMock = new Mock<ISriWebServiceClient>();
        _loggerMock = new Mock<ILogger<CheckCreditNoteAuthorizationStatusCommandHandler>>();
        _creditNoteRepositoryMock = new Mock<ICreditNoteRepository>();
        _sriErrorLogRepositoryMock = new Mock<ISriErrorLogRepository>();

        _unitOfWorkMock.Setup(u => u.CreditNotes).Returns(_creditNoteRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.SriErrorLogs).Returns(_sriErrorLogRepositoryMock.Object);

        _handler = new CheckCreditNoteAuthorizationStatusCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _sriWebServiceClientMock.Object,
            _loggerMock.Object);
    }

    // ── Credit note not found ─────────────────────────────────────────────────

    [Fact]
    public async Task Handle_CreditNoteNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((CreditNote?)null);

        var command = new CheckCreditNoteAuthorizationStatusCommand { CreditNoteId = Guid.NewGuid() };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Credit note not found");
        _sriWebServiceClientMock.Verify(c => c.CheckAuthorizationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CreditNoteFromDifferentTenant_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var creditNote = new CreditNote { TenantId = otherTenantId, Status = InvoiceStatus.PendingAuthorization };
        _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);

        var command = new CheckCreditNoteAuthorizationStatusCommand { CreditNoteId = Guid.NewGuid() };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Credit note not found");
    }

    // ── Status guard ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(InvoiceStatus.Draft)]
    [InlineData(InvoiceStatus.PendingSignature)]
    [InlineData(InvoiceStatus.Rejected)]
    public async Task Handle_WrongStatus_ShouldReturnFailure(InvoiceStatus status)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = status };
        _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);

        var command = new CheckCreditNoteAuthorizationStatusCommand { CreditNoteId = creditNoteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("cannot check authorization");
        _sriWebServiceClientMock.Verify(c => c.CheckAuthorizationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── Access key missing ────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_NoAccessKey_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = InvoiceStatus.PendingAuthorization, AccessKey = null };
        _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);

        var command = new CheckCreditNoteAuthorizationStatusCommand { CreditNoteId = creditNoteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access key not found");
        _sriWebServiceClientMock.Verify(c => c.CheckAuthorizationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── SRI Authorized response ───────────────────────────────────────────────

    [Fact]
    public async Task Handle_SriAuthorized_ShouldUpdateCreditNoteToAuthorized()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        var authorizationNumber = "4902202504019990000000001100100100000000122345671";
        var authorizationDate = DateTime.UtcNow;

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var creditNote = new CreditNote
        {
            Id = creditNoteId,
            TenantId = tenantId,
            Status = InvoiceStatus.PendingAuthorization,
            AccessKey = ValidAccessKey
        };

        _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);
        _sriWebServiceClientMock.Setup(c => c.CheckAuthorizationAsync(ValidAccessKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SriAuthorizationResponse
            {
                IsAuthorized = true,
                Status = "AUTORIZADO",
                AuthorizationNumber = authorizationNumber,
                AuthorizationDate = authorizationDate
            });

        var command = new CheckCreditNoteAuthorizationStatusCommand { CreditNoteId = creditNoteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsAuthorized.Should().BeTrue();
        creditNote.Status.Should().Be(InvoiceStatus.Authorized);
        creditNote.SriAuthorization.Should().Be(authorizationNumber);
        creditNote.AuthorizationDate.HasValue.Should().BeTrue();

        _creditNoteRepositoryMock.Verify(r => r.UpdateAsync(creditNote, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AlreadyAuthorizedCreditNote_ShouldAlsoCheckAuthorization()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = InvoiceStatus.Authorized, AccessKey = ValidAccessKey };
        _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);
        _sriWebServiceClientMock.Setup(c => c.CheckAuthorizationAsync(ValidAccessKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SriAuthorizationResponse { IsAuthorized = true, Status = "AUTORIZADO", AuthorizationNumber = "12345" });

        var command = new CheckCreditNoteAuthorizationStatusCommand { CreditNoteId = creditNoteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _sriWebServiceClientMock.Verify(c => c.CheckAuthorizationAsync(ValidAccessKey, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── EN PROCESAMIENTO — no state change ────────────────────────────────────

    [Fact]
    public async Task Handle_SriEnProcesamiento_ShouldNotChangeStatus()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = InvoiceStatus.PendingAuthorization, AccessKey = ValidAccessKey };
        _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);
        _sriWebServiceClientMock.Setup(c => c.CheckAuthorizationAsync(ValidAccessKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SriAuthorizationResponse { IsAuthorized = false, Status = "EN PROCESAMIENTO" });

        var command = new CheckCreditNoteAuthorizationStatusCommand { CreditNoteId = creditNoteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        creditNote.Status.Should().Be(InvoiceStatus.PendingAuthorization);
        _creditNoteRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<CreditNote>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── Transient errors — no state change ────────────────────────────────────

    [Theory]
    [InlineData("INVALID_RESPONSE")]
    [InlineData("PARSE_ERROR")]
    public async Task Handle_TransientError_ShouldNotChangeStatus(string errorCode)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = InvoiceStatus.PendingAuthorization, AccessKey = ValidAccessKey };
        _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);
        _sriWebServiceClientMock.Setup(c => c.CheckAuthorizationAsync(ValidAccessKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SriAuthorizationResponse
            {
                IsAuthorized = false,
                Status = "ERROR",
                Errors = new List<SriError> { new() { Code = errorCode, Message = "Transient error" } }
            });

        var command = new CheckCreditNoteAuthorizationStatusCommand { CreditNoteId = creditNoteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        creditNote.Status.Should().Be(InvoiceStatus.PendingAuthorization);
        _creditNoteRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<CreditNote>(), It.IsAny<CancellationToken>()), Times.Never);
        _sriErrorLogRepositoryMock.Verify(r => r.AddAsync(It.IsAny<SriErrorLog>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── Permanent errors — status → Rejected ─────────────────────────────────

    [Fact]
    public async Task Handle_PermanentErrors_ShouldMarkAsRejectedAndLogErrors()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = InvoiceStatus.PendingAuthorization, AccessKey = ValidAccessKey };
        _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);
        _sriWebServiceClientMock.Setup(c => c.CheckAuthorizationAsync(ValidAccessKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SriAuthorizationResponse
            {
                IsAuthorized = false,
                Status = "NO AUTORIZADO",
                Errors = new List<SriError>
                {
                    new() { Code = "NRO_49", Message = "Clave de acceso inválida" },
                    new() { Code = "NRO_60", Message = "RUC no autorizado" }
                }
            });

        var command = new CheckCreditNoteAuthorizationStatusCommand { CreditNoteId = creditNoteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        creditNote.Status.Should().Be(InvoiceStatus.Rejected);

        _sriErrorLogRepositoryMock.Verify(
            r => r.AddAsync(It.Is<SriErrorLog>(e => e.CreditNoteId == creditNoteId && e.Operation == "CheckAuthorization"), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── No tenant ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new CheckCreditNoteAuthorizationStatusCommand { CreditNoteId = Guid.NewGuid() };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
