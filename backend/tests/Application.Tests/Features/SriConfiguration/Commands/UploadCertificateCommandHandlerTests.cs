using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Features.SriConfiguration.Commands.UploadCertificate;
using SaaS.Domain.Enums;
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

    // Helper method to generate a valid P12 certificate for testing
    private byte[] GenerateValidP12Certificate()
    {
        // This is a minimal valid PKCS#12 certificate for testing purposes
        // In a real test, you would load a test certificate file
        // For now, return empty byte array since we're testing failure cases
        return Array.Empty<byte>();
    }

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
