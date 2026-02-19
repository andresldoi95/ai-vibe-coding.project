using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.DTOs;
using SaaS.Application.Features.SriConfiguration.Queries.GetSriConfiguration;
using SaaS.Domain.Enums;
using Xunit;
using SriConfigurationEntity = SaaS.Domain.Entities.SriConfiguration;

namespace Application.Tests.Features.SriConfiguration.Queries;

public class GetSriConfigurationQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetSriConfigurationQueryHandler>> _loggerMock;
    private readonly Mock<ISriConfigurationRepository> _sriConfigRepositoryMock;
    private readonly GetSriConfigurationQueryHandler _handler;

    public GetSriConfigurationQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetSriConfigurationQueryHandler>>();
        _sriConfigRepositoryMock = new Mock<ISriConfigurationRepository>();

        _unitOfWorkMock.Setup(u => u.SriConfigurations).Returns(_sriConfigRepositoryMock.Object);

        _handler = new GetSriConfigurationQueryHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ConfigurationExists_ShouldReturnConfiguration()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var configId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var sriConfig = new SriConfigurationEntity
        {
            Id = configId,
            TenantId = tenantId,
            CompanyRuc = "1234567890001",
            LegalName = "Test Company S.A.",
            TradeName = "Test Company",
            MainAddress = "Av. Test 123, Quito",
            Environment = SriEnvironment.Production,
            AccountingRequired = true,
            DigitalCertificate = new byte[] { 0x01, 0x02 },
            CertificatePassword = "test123",
            CertificateExpiryDate = DateTime.UtcNow.AddYears(1),
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow
        };

        _sriConfigRepositoryMock
            .Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sriConfig);

        var query = new GetSriConfigurationQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(configId);
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.CompanyRuc.Should().Be("1234567890001");
        result.Value.LegalName.Should().Be("Test Company S.A.");
        result.Value.TradeName.Should().Be("Test Company");
        result.Value.MainAddress.Should().Be("Av. Test 123, Quito");
        result.Value.Environment.Should().Be(SriEnvironment.Production);
        result.Value.AccountingRequired.Should().BeTrue();
        result.Value.IsCertificateConfigured.Should().BeTrue();
        result.Value.IsCertificateValid.Should().BeTrue();

        _sriConfigRepositoryMock.Verify(r => r.GetByTenantIdAsync(
            tenantId,
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoConfiguration_ShouldReturnEmptyConfiguration()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _sriConfigRepositoryMock
            .Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SriConfigurationEntity?)null);

        var query = new GetSriConfigurationQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(Guid.Empty);
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.CompanyRuc.Should().Be(string.Empty);
        result.Value.LegalName.Should().Be(string.Empty);
        result.Value.MainAddress.Should().Be(string.Empty);
        result.Value.Environment.Should().Be(SriEnvironment.Test);
        result.Value.AccountingRequired.Should().BeFalse();
        result.Value.IsCertificateConfigured.Should().BeFalse();
        result.Value.IsCertificateValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var query = new GetSriConfigurationQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");

        _sriConfigRepositoryMock.Verify(r => r.GetByTenantIdAsync(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_TestEnvironment_ShouldReturnCorrectly()
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
            Environment = SriEnvironment.Test,  // Test environment
            AccountingRequired = false
        };

        _sriConfigRepositoryMock
            .Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sriConfig);

        var query = new GetSriConfigurationQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Environment.Should().Be(SriEnvironment.Test);
    }

    [Fact]
    public async Task Handle_CertificateNotConfigured_ShouldReturnFalseFlags()
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
            Environment = SriEnvironment.Production,
            AccountingRequired = true
            // No certificate configured - computed properties will be false
        };

        _sriConfigRepositoryMock
            .Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sriConfig);

        var query = new GetSriConfigurationQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCertificateConfigured.Should().BeFalse();
        result.Value.IsCertificateValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_NullTradeName_ShouldMapCorrectly()
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
            TradeName = string.Empty,
            MainAddress = "Test Address",
            Environment = SriEnvironment.Production,
            AccountingRequired = false
        };

        _sriConfigRepositoryMock
            .Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sriConfig);

        var query = new GetSriConfigurationQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TradeName.Should().BeEmpty();
    }
}
