using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Features.CreditNotes.Commands.DeleteCreditNote;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;
using Xunit;

namespace Application.Tests.Features.CreditNotes.Commands;

public class DeleteCreditNoteCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<DeleteCreditNoteCommandHandler>> _loggerMock;
    private readonly Mock<ICreditNoteRepository> _creditNoteRepositoryMock;
    private readonly DeleteCreditNoteCommandHandler _handler;

    public DeleteCreditNoteCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<DeleteCreditNoteCommandHandler>>();
        _creditNoteRepositoryMock = new Mock<ICreditNoteRepository>();

        _unitOfWorkMock.Setup(u => u.CreditNotes).Returns(_creditNoteRepositoryMock.Object);

        _handler = new DeleteCreditNoteCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_DraftCreditNote_ShouldSoftDeleteAndReturnSuccess()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var creditNote = new CreditNote
        {
            Id = creditNoteId,
            TenantId = tenantId,
            Status = InvoiceStatus.Draft,
            IsDeleted = false
        };
        _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);

        var command = new DeleteCreditNoteCommand(creditNoteId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        creditNote.IsDeleted.Should().BeTrue();
        creditNote.DeletedAt.HasValue.Should().BeTrue();
        creditNote.UpdatedAt.Should().NotBe(default(DateTime?));

        _creditNoteRepositoryMock.Verify(r => r.UpdateAsync(creditNote, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Not found ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_CreditNoteNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((CreditNote?)null);

        var command = new DeleteCreditNoteCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Credit note not found");
        _creditNoteRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<CreditNote>(), It.IsAny<CancellationToken>()), Times.Never);
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
            .ReturnsAsync(new CreditNote { Id = creditNoteId, TenantId = otherTenantId, Status = InvoiceStatus.Draft });

        var command = new DeleteCreditNoteCommand(creditNoteId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Credit note not found");
    }

    // ── Status guard ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(InvoiceStatus.PendingSignature)]
    [InlineData(InvoiceStatus.PendingAuthorization)]
    [InlineData(InvoiceStatus.Authorized)]
    [InlineData(InvoiceStatus.Rejected)]
    [InlineData(InvoiceStatus.Cancelled)]
    public async Task Handle_NonDraftCreditNote_ShouldReturnFailure(InvoiceStatus status)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = status });

        var command = new DeleteCreditNoteCommand(creditNoteId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Only draft credit notes can be deleted");
        _creditNoteRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<CreditNote>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── No tenant ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new DeleteCreditNoteCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _creditNoteRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<CreditNote>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
