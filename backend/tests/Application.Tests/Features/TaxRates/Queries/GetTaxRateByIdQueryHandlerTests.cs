using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.TaxRates.Queries.GetTaxRateById;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.TaxRates.Queries;

public class GetTaxRateByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetTaxRateByIdQueryHandler>> _loggerMock;
    private readonly Mock<ITaxRateRepository> _taxRateRepositoryMock;
    private readonly GetTaxRateByIdQueryHandler _handler;

    public GetTaxRateByIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetTaxRateByIdQueryHandler>>();
        _taxRateRepositoryMock = new Mock<ITaxRateRepository>();

        _unitOfWorkMock.Setup(u => u.TaxRates).Returns(_taxRateRepositoryMock.Object);

        _handler = new GetTaxRateByIdQueryHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingTaxRate_ShouldReturnTaxRate()
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
            Code = "IVA12",
            Description = "Standard VAT"
        };

        _taxRateRepositoryMock
            .Setup(r => r.GetByIdAsync(taxRateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taxRate);

        var query = new GetTaxRateByIdQuery(taxRateId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(taxRateId);
        result.Value.Name.Should().Be("IVA 12%");
        result.Value.Rate.Should().Be(12.00m);
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

        var query = new GetTaxRateByIdQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tax rate not found");
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var query = new GetTaxRateByIdQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant");
    }
}
