using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Features.CreditNotes.Commands.GenerateCreditNoteRide;
using SaaS.Application.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;
using Xunit;

namespace Application.Tests.Features.CreditNotes.Commands;

public class GenerateCreditNoteRideCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ICreditNoteRideService> _creditNoteRideServiceMock;
    private readonly Mock<ILogger<GenerateCreditNoteRideCommandHandler>> _loggerMock;
    private readonly Mock<ICreditNoteRepository> _creditNoteRepositoryMock;
    private readonly Mock<ISriConfigurationRepository> _sriConfigRepositoryMock;
    private readonly Mock<IEmissionPointRepository> _emissionPointRepositoryMock;
    private readonly GenerateCreditNoteRideCommandHandler _handler;

    public GenerateCreditNoteRideCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _creditNoteRideServiceMock = new Mock<ICreditNoteRideService>();
        _loggerMock = new Mock<ILogger<GenerateCreditNoteRideCommandHandler>>();
        _creditNoteRepositoryMock = new Mock<ICreditNoteRepository>();
        _sriConfigRepositoryMock = new Mock<ISriConfigurationRepository>();
        _emissionPointRepositoryMock = new Mock<IEmissionPointRepository>();

        _unitOfWorkMock.Setup(u => u.CreditNotes).Returns(_creditNoteRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.SriConfigurations).Returns(_sriConfigRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.EmissionPoints).Returns(_emissionPointRepositoryMock.Object);

        _handler = new GenerateCreditNoteRideCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _creditNoteRideServiceMock.Object,
            _loggerMock.Object);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_AuthorizedCreditNote_ShouldGenerateRideAndUpdatePath()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        var emissionPointId = Guid.NewGuid();
        var expectedRidePath = "/ride/cn-001.pdf";

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var establishment = new Establishment { TenantId = tenantId, EstablishmentCode = "001" };
        var emissionPoint = new EmissionPoint { Id = emissionPointId, TenantId = tenantId, Establishment = establishment };
        var creditNote = new CreditNote
        {
            Id = creditNoteId,
            TenantId = tenantId,
            Status = InvoiceStatus.Authorized,
            AccessKey = "4902202504019990000000001100100100000000122345671",
            SriAuthorization = "4902202504019990000000001100100100000000122345671",
            EmissionPointId = emissionPointId,
            Items = new List<CreditNoteItem>()
        };
        var sriConfig = new SaaS.Domain.Entities.SriConfiguration { TenantId = tenantId, CompanyRuc = "1234567890001" };

        _creditNoteRepositoryMock.Setup(r => r.GetWithItemsAsync(creditNoteId, tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);
        _sriConfigRepositoryMock.Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(sriConfig);
        _emissionPointRepositoryMock.Setup(r => r.GetByIdAsync(emissionPointId, It.IsAny<CancellationToken>())).ReturnsAsync(emissionPoint);
        _creditNoteRideServiceMock.Setup(s => s.GenerateRidePdfAsync(creditNote, sriConfig, establishment, emissionPoint))
            .ReturnsAsync(expectedRidePath);

        var command = new GenerateCreditNoteRideCommand { CreditNoteId = creditNoteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedRidePath);
        creditNote.RideFilePath.Should().Be(expectedRidePath);

        _creditNoteRepositoryMock.Verify(r => r.UpdateAsync(creditNote, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _creditNoteRideServiceMock.Verify(s => s.GenerateRidePdfAsync(It.IsAny<CreditNote>(), It.IsAny<SaaS.Domain.Entities.SriConfiguration>(), It.IsAny<Establishment>(), It.IsAny<EmissionPoint>()), Times.Once);
    }

    // ── Credit note not found ─────────────────────────────────────────────────

    [Fact]
    public async Task Handle_CreditNoteNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _creditNoteRepositoryMock.Setup(r => r.GetWithItemsAsync(It.IsAny<Guid>(), tenantId, It.IsAny<CancellationToken>())).ReturnsAsync((CreditNote?)null);

        var command = new GenerateCreditNoteRideCommand { CreditNoteId = Guid.NewGuid() };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Credit note not found");
        _creditNoteRideServiceMock.Verify(s => s.GenerateRidePdfAsync(It.IsAny<CreditNote>(), It.IsAny<SaaS.Domain.Entities.SriConfiguration>(), It.IsAny<Establishment>(), It.IsAny<EmissionPoint>()), Times.Never);
    }

    // ── Status guard ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(InvoiceStatus.Draft)]
    [InlineData(InvoiceStatus.PendingSignature)]
    [InlineData(InvoiceStatus.PendingAuthorization)]
    [InlineData(InvoiceStatus.Rejected)]
    public async Task Handle_NonAuthorizedStatus_ShouldReturnFailure(InvoiceStatus status)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var creditNote = new CreditNote
        {
            Id = creditNoteId,
            TenantId = tenantId,
            Status = status,
            AccessKey = "4902202504019990000000001100100100000000122345671",
            SriAuthorization = "AUTH-123",
            Items = new List<CreditNoteItem>()
        };
        _creditNoteRepositoryMock.Setup(r => r.GetWithItemsAsync(creditNoteId, tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);

        var command = new GenerateCreditNoteRideCommand { CreditNoteId = creditNoteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("authorized credit notes");
    }

    // ── Access key missing ────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_NoAccessKey_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = InvoiceStatus.Authorized, AccessKey = null, SriAuthorization = "AUTH-123", Items = new List<CreditNoteItem>() };
        _creditNoteRepositoryMock.Setup(r => r.GetWithItemsAsync(creditNoteId, tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);

        var command = new GenerateCreditNoteRideCommand { CreditNoteId = creditNoteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access key");
    }

    // ── SRI authorization number missing ──────────────────────────────────────

    [Fact]
    public async Task Handle_NoSriAuthorization_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = InvoiceStatus.Authorized, AccessKey = "4902202504019990000000001100100100000000122345671", SriAuthorization = null, Items = new List<CreditNoteItem>() };
        _creditNoteRepositoryMock.Setup(r => r.GetWithItemsAsync(creditNoteId, tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);

        var command = new GenerateCreditNoteRideCommand { CreditNoteId = creditNoteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("SRI authorization number");
    }

    // ── SRI config missing ────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_SriConfigNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = InvoiceStatus.Authorized, AccessKey = "4902202504019990000000001100100100000000122345671", SriAuthorization = "AUTH-123", EmissionPointId = Guid.NewGuid(), Items = new List<CreditNoteItem>() };
        _creditNoteRepositoryMock.Setup(r => r.GetWithItemsAsync(creditNoteId, tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);
        _sriConfigRepositoryMock.Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync((SaaS.Domain.Entities.SriConfiguration?)null);

        var command = new GenerateCreditNoteRideCommand { CreditNoteId = creditNoteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("SRI configuration not found");
    }

    // ── No tenant ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new GenerateCreditNoteRideCommand { CreditNoteId = Guid.NewGuid() };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
