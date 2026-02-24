using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Features.SriConfiguration.Commands.UploadCertificate;
using SaaS.Domain.Enums;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Xunit;
using SriConfigurationEntity = SaaS.Domain.Entities.SriConfiguration;

namespace Application.Tests.Features.SriConfiguration.Commands;

public class UploadCertificateCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<UploadCertificateCommandHandler>> _loggerMock;
    private readonly Mock<ISriConfigurationRepository> _sriConfigRepositoryMock;
    private readonly UploadCertificateCommandHandler _handler;

    public UploadCertificateCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<UploadCertificateCommandHandler>>();
        _sriConfigRepositoryMock = new Mock<ISriConfigurationRepository>();

        _unitOfWorkMock.Setup(u => u.SriConfigurations).Returns(_sriConfigRepositoryMock.Object);

        _handler = new UploadCertificateCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new UploadCertificateCommand
        {
            CertificateFile = new byte[] { 0x01, 0x02, 0x03 },
            CertificatePassword = "password"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");

        _sriConfigRepositoryMock.Verify(r => r.UpdateAsync(
            It.IsAny<SriConfigurationEntity>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_NoConfigurationExists_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _sriConfigRepositoryMock
            .Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SriConfigurationEntity?)null);

        var command = new UploadCertificateCommand
        {
            CertificateFile = GenerateValidP12Certificate(),
            CertificatePassword = "test"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert -  Handler checks certificate validity before checking configuration exists
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid certificate");

        _sriConfigRepositoryMock.Verify(r => r.UpdateAsync(
            It.IsAny<SriConfigurationEntity>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_InvalidCertificateData_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var sriConfig = new SriConfigurationEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CompanyRuc = "1234567890001",
            LegalName = "Test Company",
            MainAddress = "Test Address",
            Environment = SriEnvironment.Production
        };

        _sriConfigRepositoryMock
            .Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sriConfig);

        var command = new UploadCertificateCommand
        {
            CertificateFile = new byte[] { 0x00, 0x01, 0x02 },  // Invalid certificate data
            CertificatePassword = "wrong-password"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid certificate or password");

        _sriConfigRepositoryMock.Verify(r => r.UpdateAsync(
            It.IsAny<SriConfigurationEntity>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // -----------------------------------------------------------------------
    // Expired certificate
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ExpiredCertificate_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        const string password = "test123";
        var expiredCert = GenerateSelfSignedPfx(password, DateTimeOffset.UtcNow.AddDays(-1));

        var command = new UploadCertificateCommand
        {
            CertificateFile = expiredCert,
            CertificatePassword = password
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("expired");

        _sriConfigRepositoryMock.Verify(r => r.UpdateAsync(
            It.IsAny<SriConfigurationEntity>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // -----------------------------------------------------------------------
    // Valid certificate but no SRI configuration exists
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ValidCertificateButNoSriConfig_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _sriConfigRepositoryMock
            .Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SriConfigurationEntity?)null);

        const string password = "test456";
        var validCert = GenerateSelfSignedPfx(password, DateTimeOffset.UtcNow.AddYears(1));

        var command = new UploadCertificateCommand
        {
            CertificateFile = validCert,
            CertificatePassword = password
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("SRI configuration not found");

        _sriConfigRepositoryMock.Verify(r => r.UpdateAsync(
            It.IsAny<SriConfigurationEntity>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // -----------------------------------------------------------------------
    // Success — valid certificate + existing SRI configuration
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ValidCertificateWithExistingConfig_ShouldUpdateAndReturnDto()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var configId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var sriConfig = new SriConfigurationEntity
        {
            Id = configId,
            TenantId = tenantId,
            CompanyRuc = "1234567897001",
            LegalName = "ACME Ecuador S.A.",
            MainAddress = "Av. Test 123, Quito",
            Environment = SriEnvironment.Test,
            AccountingRequired = false
        };

        _sriConfigRepositoryMock
            .Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sriConfig);

        const string password = "cert-password";
        var validCert = GenerateSelfSignedPfx(password, DateTimeOffset.UtcNow.AddYears(2));

        var command = new UploadCertificateCommand
        {
            CertificateFile = validCert,
            CertificatePassword = password
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TenantId.Should().Be(tenantId);
        result.Value.CompanyRuc.Should().Be("1234567897001");
        result.Value.CertificateExpiryDate.Should().HaveValue();

        // Certificate data stored on entity
        sriConfig.DigitalCertificate.Should().BeEquivalentTo(validCert);
        sriConfig.CertificatePassword.Should().Be(password);
        sriConfig.CertificateExpiryDate.Should().HaveValue();
        sriConfig.CertificateExpiryDate!.Value.Should().BeAfter(DateTime.UtcNow);

        _sriConfigRepositoryMock.Verify(r => r.UpdateAsync(
            It.Is<SriConfigurationEntity>(s => s.Id == configId),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // -----------------------------------------------------------------------
    // Helper methods
    // -----------------------------------------------------------------------

    /// <summary>
    /// Generates a self-signed PFX/PKCS#12 certificate for testing.
    /// notBefore is fixed at 2 years ago so an expired notAfter (e.g. yesterday) is still valid to construct.
    /// </summary>
    private static byte[] GenerateSelfSignedPfx(string password, DateTimeOffset notAfter)
    {
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest(
            "CN=TestCert",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        // Use a fixed notBefore 2 years in the past so notAfter can be either
        // in the past (expired) or the future (valid) without violating the
        // "notBefore must be <= notAfter" constraint.
        var notBefore = DateTimeOffset.UtcNow.AddYears(-2);
        var cert = req.CreateSelfSigned(notBefore, notAfter);
        return cert.Export(X509ContentType.Pfx, password);
    }

    // Keep the old helper method for backward compatibility with existing tests
    private static byte[] GenerateValidP12Certificate() => Array.Empty<byte>();

    [Fact]
    public async Task Handle_ConfigurationExistsButInvalidCertificate_ShouldStillFailValidation()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var sriConfig = new SriConfigurationEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CompanyRuc = "1234567890001",
            LegalName = "Test Company S.A.",
            MainAddress = "Test Address",
            Environment = SriEnvironment.Test,
            AccountingRequired = false
        };

        _sriConfigRepositoryMock
            .Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sriConfig);

        var command = new UploadCertificateCommand
        {
            CertificateFile = new byte[] { 0xFF, 0xFE },  // Invalid data
            CertificatePassword = "test123"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid certificate");
    }

    [Fact]
    public async Task Handle_EmptyCertificateFile_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var sriConfig = new SriConfigurationEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CompanyRuc = "1234567890001",
            LegalName = "Test Company",
            MainAddress = "Test Address",
            Environment = SriEnvironment.Production
        };

        _sriConfigRepositoryMock
            .Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sriConfig);

        var command = new UploadCertificateCommand
        {
            CertificateFile = Array.Empty<byte>(),  // Empty file
            CertificatePassword = "password"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid certificate");
    }
}
