using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.DTOs;
using SaaS.Application.Features.SriConfiguration.Commands.UpdateSriConfiguration;
using SaaS.Domain.Enums;
using Xunit;
using SriConfigurationEntity = SaaS.Domain.Entities.SriConfiguration;

namespace Application.Tests.Features.SriConfiguration.Commands;

public class UpdateSriConfigurationCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<UpdateSriConfigurationCommandHandler>> _loggerMock;
    private readonly Mock<ISriConfigurationRepository> _sriConfigRepositoryMock;
    private readonly UpdateSriConfigurationCommandHandler _handler;

    public UpdateSriConfigurationCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<UpdateSriConfigurationCommandHandler>>();
        _sriConfigRepositoryMock = new Mock<ISriConfigurationRepository>();

        _unitOfWorkMock.Setup(u => u.SriConfigurations).Returns(_sriConfigRepositoryMock.Object);

        _handler = new UpdateSriConfigurationCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_CreateNew_ShouldCreateConfiguration()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _sriConfigRepositoryMock
            .Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SriConfigurationEntity?)null);

        var command = new UpdateSriConfigurationCommand
        {
            CompanyRuc = "1234567890001",
            LegalName = "Test Company S.A.",
            TradeName = "Test Company",
            MainAddress = "Av. Test 123, Quito",
            Environment = SriEnvironment.Production,
            AccountingRequired = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.CompanyRuc.Should().Be("1234567890001");
        result.Value.LegalName.Should().Be("Test Company S.A.");
        result.Value.TenantId.Should().Be(tenantId);

        _sriConfigRepositoryMock.Verify(r => r.AddAsync(
            It.Is<SriConfigurationEntity>(s =>
                s.CompanyRuc == "1234567890001" &&
                s.TenantId == tenantId &&
                s.Environment == SriEnvironment.Production),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UpdateExisting_ShouldUpdateConfiguration()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var configId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var existingConfig = new SriConfigurationEntity
        {
            Id = configId,
            TenantId = tenantId,
            CompanyRuc = "0000000000001",
            LegalName = "Old Company",
            TradeName = "Old Trade",
            MainAddress = "Old Address",
            Environment = SriEnvironment.Test,
            AccountingRequired = false
        };

        _sriConfigRepositoryMock
            .Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingConfig);

        var command = new UpdateSriConfigurationCommand
        {
            CompanyRuc = "1234567890001",
            LegalName = "Updated Company S.A.",
            TradeName = "Updated Trade",
            MainAddress = "Updated Address",
            Environment = SriEnvironment.Production,
            AccountingRequired = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.CompanyRuc.Should().Be("1234567890001");
        result.Value.LegalName.Should().Be("Updated Company S.A.");
        result.Value.Environment.Should().Be(SriEnvironment.Production);
        result.Value.AccountingRequired.Should().BeTrue();

        _sriConfigRepositoryMock.Verify(r => r.UpdateAsync(
            It.Is<SriConfigurationEntity>(s =>
                s.Id == configId &&
                s.CompanyRuc == "1234567890001" &&
                s.AccountingRequired == true),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new UpdateSriConfigurationCommand
        {
            CompanyRuc = "1234567890001",
            LegalName = "Test Company",
            MainAddress = "Test Address",
            Environment = SriEnvironment.Test,
            AccountingRequired = false
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");

        _sriConfigRepositoryMock.Verify(r => r.AddAsync(
            It.IsAny<SriConfigurationEntity>(),
            It.IsAny<CancellationToken>()),
            Times.Never);

        _sriConfigRepositoryMock.Verify(r => r.UpdateAsync(
            It.IsAny<SriConfigurationEntity>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_TestEnvironment_ShouldCreate()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _sriConfigRepositoryMock
            .Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SriConfigurationEntity?)null);

        var command = new UpdateSriConfigurationCommand
        {
            CompanyRuc = "1234567890001",
            LegalName = "Test Company",
            MainAddress = "Test Address",
            Environment = SriEnvironment.Test,
            AccountingRequired = false
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Environment.Should().Be(SriEnvironment.Test);
    }

    [Fact]
    public async Task Handle_WithoutTradeName_ShouldSucceed()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _sriConfigRepositoryMock
            .Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SriConfigurationEntity?)null);

        var command = new UpdateSriConfigurationCommand
        {
            CompanyRuc = "1234567890001",
            LegalName = "Test Company S.A.",
            TradeName = "",  // Optional field
            MainAddress = "Test Address",
            Environment = SriEnvironment.Production,
            AccountingRequired = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TradeName.Should().Be("");
    }
}
