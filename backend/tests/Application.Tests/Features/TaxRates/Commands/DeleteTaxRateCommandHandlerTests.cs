using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.TaxRates.Commands.DeleteTaxRate;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.TaxRates.Commands;

public class DeleteTaxRateCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<DeleteTaxRateCommandHandler>> _loggerMock;
    private readonly Mock<ITaxRateRepository> _taxRateRepositoryMock;
    private readonly DeleteTaxRateCommandHandler _handler;

    public DeleteTaxRateCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<DeleteTaxRateCommandHandler>>();
        _taxRateRepositoryMock = new Mock<ITaxRateRepository>();

        _unitOfWorkMock.Setup(u => u.TaxRates).Returns(_taxRateRepositoryMock.Object);

        _handler = new DeleteTaxRateCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingTaxRate_ShouldDeleteSuccessfully()
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
            Rate = 12.00m,
            IsDeleted = false
        };

        _taxRateRepositoryMock
            .Setup(r => r.GetByIdAsync(taxRateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taxRate);

        var command = new DeleteTaxRateCommand(taxRateId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        taxRate.IsDeleted.Should().BeTrue();
        taxRate.DeletedAt.Should().NotBeNull();

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

        var command = new DeleteTaxRateCommand(Guid.NewGuid());

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

        var command = new DeleteTaxRateCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant");
    }
}
