using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Features.CreditNotes.Commands.SignCreditNote;
using SaaS.Application.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;
using Xunit;

namespace Application.Tests.Features.CreditNotes.Commands;

public class SignCreditNoteCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IXmlSignatureService> _xmlSignatureServiceMock;
    private readonly Mock<ILogger<SignCreditNoteCommandHandler>> _loggerMock;
    private readonly Mock<ICreditNoteRepository> _creditNoteRepositoryMock;
    private readonly Mock<ISriConfigurationRepository> _sriConfigRepositoryMock;
    private readonly SignCreditNoteCommandHandler _handler;

    public SignCreditNoteCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _xmlSignatureServiceMock = new Mock<IXmlSignatureService>();
        _loggerMock = new Mock<ILogger<SignCreditNoteCommandHandler>>();
        _creditNoteRepositoryMock = new Mock<ICreditNoteRepository>();
        _sriConfigRepositoryMock = new Mock<ISriConfigurationRepository>();

        _unitOfWorkMock.Setup(u => u.CreditNotes).Returns(_creditNoteRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.SriConfigurations).Returns(_sriConfigRepositoryMock.Object);

        _handler = new SignCreditNoteCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _xmlSignatureServiceMock.Object,
            _loggerMock.Object);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(InvoiceStatus.PendingSignature)]
    [InlineData(InvoiceStatus.PendingAuthorization)]
    public async Task Handle_PendingSignatureCreditNote_ShouldSignAndUpdateStatus(InvoiceStatus status)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        var xmlPath = Path.GetTempFileName();
        var signedXmlPath = Path.GetTempFileName();

        try
        {
            _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

            var creditNote = new CreditNote
            {
                Id = creditNoteId,
                TenantId = tenantId,
                Status = status,
                XmlFilePath = xmlPath
            };
            var sriConfig = new SaaS.Domain.Entities.SriConfiguration
            {
                TenantId = tenantId,
                CompanyRuc = "1234567890001",
                DigitalCertificate = new byte[] { 1, 2, 3 },
                CertificatePassword = "test-password",
                CertificateExpiryDate = DateTime.UtcNow.AddYears(1)
            };

            _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);
            _sriConfigRepositoryMock.Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(sriConfig);
            _xmlSignatureServiceMock.Setup(s => s.SignXmlAsync(xmlPath, sriConfig.DigitalCertificate, sriConfig.CertificatePassword))
                .ReturnsAsync(signedXmlPath);

            var command = new SignCreditNoteCommand { CreditNoteId = creditNoteId };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(signedXmlPath);
            creditNote.SignedXmlFilePath.Should().Be(signedXmlPath);
            creditNote.Status.Should().Be(InvoiceStatus.PendingAuthorization);

            _creditNoteRepositoryMock.Verify(r => r.UpdateAsync(creditNote, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            File.Delete(xmlPath);
            File.Delete(signedXmlPath);
        }
    }

    // ── Credit note not found ─────────────────────────────────────────────────

    [Fact]
    public async Task Handle_CreditNoteNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((CreditNote?)null);

        var command = new SignCreditNoteCommand { CreditNoteId = Guid.NewGuid() };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Credit note not found");
        _xmlSignatureServiceMock.Verify(s => s.SignXmlAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never);
    }

    // ── Status guard ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(InvoiceStatus.Draft)]
    [InlineData(InvoiceStatus.Authorized)]
    [InlineData(InvoiceStatus.Rejected)]
    public async Task Handle_WrongStatus_ShouldReturnFailure(InvoiceStatus status)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = status, XmlFilePath = "/some/path.xml" };
        _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);

        var command = new SignCreditNoteCommand { CreditNoteId = creditNoteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("PendingSignature");
        _xmlSignatureServiceMock.Verify(s => s.SignXmlAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never);
    }

    // ── XML file missing ──────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_XmlFilePathNull_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = InvoiceStatus.PendingSignature, XmlFilePath = null };
        _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);

        var command = new SignCreditNoteCommand { CreditNoteId = creditNoteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("XML file must be generated");
    }

    [Fact]
    public async Task Handle_XmlFileNotOnDisk_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = InvoiceStatus.PendingSignature, XmlFilePath = "/nonexistent/path/cn.xml" };
        _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);

        var command = new SignCreditNoteCommand { CreditNoteId = creditNoteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("XML file not found on disk");
    }

    // ── Certificate validation ────────────────────────────────────────────────

    [Fact]
    public async Task Handle_SriConfigNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        var xmlPath = Path.GetTempFileName();
        try
        {
            _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

            var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = InvoiceStatus.PendingSignature, XmlFilePath = xmlPath };
            _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);
            _sriConfigRepositoryMock.Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync((SaaS.Domain.Entities.SriConfiguration?)null);

            var command = new SignCreditNoteCommand { CreditNoteId = creditNoteId };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("SRI configuration not found");
        }
        finally { File.Delete(xmlPath); }
    }

    [Fact]
    public async Task Handle_NoCertificate_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        var xmlPath = Path.GetTempFileName();
        try
        {
            _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
            var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = InvoiceStatus.PendingSignature, XmlFilePath = xmlPath };
            _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);
            _sriConfigRepositoryMock.Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SaaS.Domain.Entities.SriConfiguration { TenantId = tenantId, DigitalCertificate = null });

            var command = new SignCreditNoteCommand { CreditNoteId = creditNoteId };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Digital certificate not found");
        }
        finally { File.Delete(xmlPath); }
    }

    [Fact]
    public async Task Handle_ExpiredCertificate_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        var xmlPath = Path.GetTempFileName();
        try
        {
            _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
            var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = InvoiceStatus.PendingSignature, XmlFilePath = xmlPath };
            _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);
            _sriConfigRepositoryMock.Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SaaS.Domain.Entities.SriConfiguration
                {
                    TenantId = tenantId,
                    DigitalCertificate = new byte[] { 1, 2, 3 },
                    CertificatePassword = "pwd",
                    CertificateExpiryDate = DateTime.UtcNow.AddDays(-1)  // expired
                });

            var command = new SignCreditNoteCommand { CreditNoteId = creditNoteId };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("expired");
        }
        finally { File.Delete(xmlPath); }
    }

    // ── Signing service exception ─────────────────────────────────────────────

    [Fact]
    public async Task Handle_SignatureServiceThrows_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        var xmlPath = Path.GetTempFileName();
        try
        {
            _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
            var creditNote = new CreditNote { Id = creditNoteId, TenantId = tenantId, Status = InvoiceStatus.PendingSignature, XmlFilePath = xmlPath };
            var sriConfig = new SaaS.Domain.Entities.SriConfiguration
            {
                TenantId = tenantId,
                DigitalCertificate = new byte[] { 1, 2, 3 },
                CertificatePassword = "pwd",
                CertificateExpiryDate = DateTime.UtcNow.AddYears(1)
            };
            _creditNoteRepositoryMock.Setup(r => r.GetByIdAsync(creditNoteId, It.IsAny<CancellationToken>())).ReturnsAsync(creditNote);
            _sriConfigRepositoryMock.Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(sriConfig);
            _xmlSignatureServiceMock.Setup(s => s.SignXmlAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Signing failed"));

            var command = new SignCreditNoteCommand { CreditNoteId = creditNoteId };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("error occurred");
        }
        finally { File.Delete(xmlPath); }
    }
}
