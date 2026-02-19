using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.TaxRates.Queries.GetAllTaxRates;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.TaxRates.Queries;

public class GetAllTaxRatesQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetAllTaxRatesQueryHandler>> _loggerMock;
    private readonly Mock<ITaxRateRepository> _taxRateRepositoryMock;
    private readonly GetAllTaxRatesQueryHandler _handler;

    public GetAllTaxRatesQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetAllTaxRatesQueryHandler>>();
        _taxRateRepositoryMock = new Mock<ITaxRateRepository>();

        _unitOfWorkMock.Setup(u => u.TaxRates).Returns(_taxRateRepositoryMock.Object);

        _handler = new GetAllTaxRatesQueryHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnTaxRates()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var taxRates = new List<TaxRate>
        {
            new TaxRate { Id = Guid.NewGuid(), TenantId = tenantId, Name = "IVA 12%", Rate = 12.00m },
            new TaxRate { Id = Guid.NewGuid(), TenantId = tenantId, Name = "IVA 0%", Rate = 0.00m }
        };

        _taxRateRepositoryMock
            .Setup(r => r.GetActiveByTenantAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taxRates);

        var query = new GetAllTaxRatesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var query = new GetAllTaxRatesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant");
    }
}
