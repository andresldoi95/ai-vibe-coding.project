using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Features.CreditNotes.Commands.GenerateCreditNoteXml;
using SaaS.Application.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;
using Xunit;

namespace Application.Tests.Features.CreditNotes.Commands;

public class GenerateCreditNoteXmlCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ICreditNoteXmlService> _creditNoteXmlServiceMock;
    private readonly Mock<ILogger<GenerateCreditNoteXmlCommandHandler>> _loggerMock;
    private readonly Mock<ICreditNoteRepository> _creditNoteRepositoryMock;
    private readonly Mock<ISriConfigurationRepository> _sriConfigRepositoryMock;
    private readonly Mock<IEmissionPointRepository> _emissionPointRepositoryMock;
    private readonly GenerateCreditNoteXmlCommandHandler _handler;

    public GenerateCreditNoteXmlCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _creditNoteXmlServiceMock = new Mock<ICreditNoteXmlService>();
        _loggerMock = new Mock<ILogger<GenerateCreditNoteXmlCommandHandler>>();
        _creditNoteRepositoryMock = new Mock<ICreditNoteRepository>();
        _sriConfigRepositoryMock = new Mock<ISriConfigurationRepository>();
        _emissionPointRepositoryMock = new Mock<IEmissionPointRepository>();

        _unitOfWorkMock.Setup(u => u.CreditNotes).Returns(_creditNoteRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.SriConfigurations).Returns(_sriConfigRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.EmissionPoints).Returns(_emissionPointRepositoryMock.Object);

        _handler = new GenerateCreditNoteXmlCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _creditNoteXmlServiceMock.Object,
            _loggerMock.Object);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidDraftCreditNote_ShouldGenerateXmlAndUpdateStatus()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        var emissionPointId = Guid.NewGuid();
        var expectedXmlPath = "/xml/credit-notes/cn-001.xml";
        var expectedAccessKey = "4902202504019990000000001100100100000000122345671";

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var establishment = new Establishment { TenantId = tenantId, EstablishmentCode = "001" };
        var emissionPoint = new EmissionPoint { Id = emissionPointId, TenantId = tenantId, Establishment = establishment };
        var creditNote = new CreditNote
        {
            Id = creditNoteId,
            TenantId = tenantId,
            Status = InvoiceStatus.Draft,
            EmissionPointId = emissionPointId,
            Items = new List<CreditNoteItem>()
        };
        var sriConfig = new SaaS.Domain.Entities.SriConfiguration { TenantId = tenantId, CompanyRuc = "1234567890001" };

        _creditNoteRepositoryMock.Setup(r => r.GetWithItemsAsync(creditNoteId, tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);
        _sriConfigRepositoryMock.Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(sriConfig);
        _emissionPointRepositoryMock.Setup(r => r.GetByIdAsync(emissionPointId, It.IsAny<CancellationToken>())).ReturnsAsync(emissionPoint);
        _creditNoteXmlServiceMock.Setup(s => s.GenerateCreditNoteXmlAsync(creditNote, sriConfig, establishment, emissionPoint))
            .ReturnsAsync((expectedXmlPath, expectedAccessKey));

        var command = new GenerateCreditNoteXmlCommand { CreditNoteId = creditNoteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedXmlPath);
        creditNote.XmlFilePath.Should().Be(expectedXmlPath);
        creditNote.AccessKey.Should().Be(expectedAccessKey);
        creditNote.Status.Should().Be(InvoiceStatus.PendingSignature);

        _creditNoteRepositoryMock.Verify(r => r.UpdateAsync(creditNote, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(InvoiceStatus.Draft)]
    [InlineData(InvoiceStatus.PendingSignature)]
    [InlineData(InvoiceStatus.PendingAuthorization)]
    public async Task Handle_AllowedStatuses_ShouldGenerateXml(InvoiceStatus status)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        var emissionPointId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var establishment = new Establishment { EstablishmentCode = "001" };
        var emissionPoint = new EmissionPoint { Id = emissionPointId, TenantId = tenantId, Establishment = establishment };
        var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = status, EmissionPointId = emissionPointId, Items = new List<CreditNoteItem>() };
        var sriConfig = new SaaS.Domain.Entities.SriConfiguration { TenantId = tenantId, CompanyRuc = "1234567890001" };

        _creditNoteRepositoryMock.Setup(r => r.GetWithItemsAsync(creditNoteId, tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);
        _sriConfigRepositoryMock.Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(sriConfig);
        _emissionPointRepositoryMock.Setup(r => r.GetByIdAsync(emissionPointId, It.IsAny<CancellationToken>())).ReturnsAsync(emissionPoint);
        _creditNoteXmlServiceMock.Setup(s => s.GenerateCreditNoteXmlAsync(It.IsAny<CreditNote>(), It.IsAny<SaaS.Domain.Entities.SriConfiguration>(), It.IsAny<Establishment>(), It.IsAny<EmissionPoint>()))
            .ReturnsAsync(("/xml/cn.xml", "12345678901234567890123456789012345678901234567890"));

        var command = new GenerateCreditNoteXmlCommand { CreditNoteId = creditNoteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    // ── Credit note not found ─────────────────────────────────────────────────

    [Fact]
    public async Task Handle_CreditNoteNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _creditNoteRepositoryMock.Setup(r => r.GetWithItemsAsync(It.IsAny<Guid>(), tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreditNote?)null);

        var command = new GenerateCreditNoteXmlCommand { CreditNoteId = Guid.NewGuid() };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Credit note not found");
        _creditNoteXmlServiceMock.Verify(s => s.GenerateCreditNoteXmlAsync(It.IsAny<CreditNote>(), It.IsAny<SaaS.Domain.Entities.SriConfiguration>(), It.IsAny<Establishment>(), It.IsAny<EmissionPoint>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CreditNoteFromDifferentTenant_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var creditNote = new CreditNote { TenantId = otherTenantId, Status = InvoiceStatus.Draft, Items = new List<CreditNoteItem>() };
        _creditNoteRepositoryMock.Setup(r => r.GetWithItemsAsync(It.IsAny<Guid>(), tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(creditNote);

        var command = new GenerateCreditNoteXmlCommand { CreditNoteId = Guid.NewGuid() };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Credit note not found");
    }

    // ── Status guard ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(InvoiceStatus.Authorized)]
    [InlineData(InvoiceStatus.Rejected)]
    [InlineData(InvoiceStatus.Cancelled)]
    public async Task Handle_DisallowedStatus_ShouldReturnFailure(InvoiceStatus status)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = status, Items = new List<CreditNoteItem>() };
        _creditNoteRepositoryMock.Setup(r => r.GetWithItemsAsync(creditNoteId, tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);

        var sriConfig = new SaaS.Domain.Entities.SriConfiguration { TenantId = tenantId, CompanyRuc = "1234567890001" };
        _sriConfigRepositoryMock.Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(sriConfig);

        var command = new GenerateCreditNoteXmlCommand { CreditNoteId = creditNoteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Cannot generate XML");
    }

    // ── SRI config missing ────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_SriConfigNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = InvoiceStatus.Draft, EmissionPointId = Guid.NewGuid(), Items = new List<CreditNoteItem>() };
        _creditNoteRepositoryMock.Setup(r => r.GetWithItemsAsync(creditNoteId, tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);
        _sriConfigRepositoryMock.Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync((SaaS.Domain.Entities.SriConfiguration?)null);

        var command = new GenerateCreditNoteXmlCommand { CreditNoteId = creditNoteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("SRI configuration not found");
    }

    // ── Service exception ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_XmlServiceThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        var emissionPointId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var establishment = new Establishment { EstablishmentCode = "001" };
        var emissionPoint = new EmissionPoint { Id = emissionPointId, TenantId = tenantId, Establishment = establishment };
        var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = InvoiceStatus.Draft, EmissionPointId = emissionPointId, Items = new List<CreditNoteItem>() };
        var sriConfig = new SaaS.Domain.Entities.SriConfiguration { TenantId = tenantId, CompanyRuc = "1234567890001" };

        _creditNoteRepositoryMock.Setup(r => r.GetWithItemsAsync(creditNoteId, tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);
        _sriConfigRepositoryMock.Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(sriConfig);
        _emissionPointRepositoryMock.Setup(r => r.GetByIdAsync(emissionPointId, It.IsAny<CancellationToken>())).ReturnsAsync(emissionPoint);
        _creditNoteXmlServiceMock.Setup(s => s.GenerateCreditNoteXmlAsync(It.IsAny<CreditNote>(), It.IsAny<SaaS.Domain.Entities.SriConfiguration>(), It.IsAny<Establishment>(), It.IsAny<EmissionPoint>()))
            .ThrowsAsync(new Exception("XML generation failed"));

        var command = new GenerateCreditNoteXmlCommand { CreditNoteId = creditNoteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("error occurred");
    }

    // ── No tenant ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new GenerateCreditNoteXmlCommand { CreditNoteId = Guid.NewGuid() };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
