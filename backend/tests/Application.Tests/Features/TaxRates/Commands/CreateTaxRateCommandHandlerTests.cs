using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.TaxRates.Commands.CreateTaxRate;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.TaxRates.Commands;

public class CreateTaxRateCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<CreateTaxRateCommandHandler>> _loggerMock;
    private readonly Mock<ITaxRateRepository> _taxRateRepositoryMock;
    private readonly CreateTaxRateCommandHandler _handler;

    public CreateTaxRateCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<CreateTaxRateCommandHandler>>();
        _taxRateRepositoryMock = new Mock<ITaxRateRepository>();

        _unitOfWorkMock.Setup(u => u.TaxRates).Returns(_taxRateRepositoryMock.Object);

        _handler = new CreateTaxRateCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateTaxRate()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var command = new CreateTaxRateCommand
        {
            Name = "IVA 12%",
            Rate = 12.00m,
            Code = "IVA12",
            Description = "Standard VAT rate"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("IVA 12%");
        result.Value.Rate.Should().Be(12.00m);

        _taxRateRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TaxRate>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new CreateTaxRateCommand
        {
            Name = "IVA 12%",
            Rate = 12.00m
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant");
    }
}
