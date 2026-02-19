using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.TaxRates.Commands.UpdateTaxRate;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.TaxRates.Commands;

public class UpdateTaxRateCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<UpdateTaxRateCommandHandler>> _loggerMock;
    private readonly Mock<ITaxRateRepository> _taxRateRepositoryMock;
    private readonly UpdateTaxRateCommandHandler _handler;

    public UpdateTaxRateCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<UpdateTaxRateCommandHandler>>();
        _taxRateRepositoryMock = new Mock<ITaxRateRepository>();

        _unitOfWorkMock.Setup(u => u.TaxRates).Returns(_taxRateRepositoryMock.Object);

        _handler = new UpdateTaxRateCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateTaxRate()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var taxRateId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var taxRate = new TaxRate
        {
            Id = taxRateId,
            TenantId = tenantId,
            Name = "IVA 12%",
            Rate = 12.00m
        };

        _taxRateRepositoryMock
            .Setup(r => r.GetByIdAsync(taxRateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taxRate);

        var command = new UpdateTaxRateCommand
        {
            Id = taxRateId,
            Name = "IVA 15%",
            Rate = 15.00m,
            Code = "IVA15",
            Description = "Updated rate"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        taxRate.Name.Should().Be("IVA 15%");
        taxRate.Rate.Should().Be(15.00m);

        _taxRateRepositoryMock.Verify(r => r.UpdateAsync(taxRate, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TaxRateNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _taxRateRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaxRate?)null);

        var command = new UpdateTaxRateCommand
        {
            Id = Guid.NewGuid(),
            Name = "IVA 15%",
            Rate = 15.00m
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tax rate not found");
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new UpdateTaxRateCommand
        {
            Id = Guid.NewGuid(),
            Name = "IVA 15%",
            Rate = 15.00m
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant");
    }
}
