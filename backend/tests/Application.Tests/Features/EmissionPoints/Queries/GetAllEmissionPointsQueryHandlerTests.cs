using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.DTOs;
using SaaS.Application.Features.EmissionPoints.Queries.GetAllEmissionPoints;
using SaaS.Domain.Entities;
using Xunit;

namespace Application.Tests.Features.EmissionPoints.Queries;

public class GetAllEmissionPointsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetAllEmissionPointsQueryHandler>> _loggerMock;
    private readonly Mock<IEmissionPointRepository> _emissionPointRepositoryMock;
    private readonly GetAllEmissionPointsQueryHandler _handler;

    public GetAllEmissionPointsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetAllEmissionPointsQueryHandler>>();
        _emissionPointRepositoryMock = new Mock<IEmissionPointRepository>();

        _unitOfWorkMock.Setup(u => u.EmissionPoints).Returns(_emissionPointRepositoryMock.Object);

        _handler = new GetAllEmissionPointsQueryHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnAllEmissionPoints()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var establishmentId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var establishment = new Establishment
        {
            Id = establishmentId,
            TenantId = tenantId,
            EstablishmentCode = "001",
            Name = "Main Office",
            Address = "123 Main St",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };

        var emissionPoints = new List<EmissionPoint>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EstablishmentId = establishmentId,
                Establishment = establishment,
                EmissionPointCode = "001",
                Name = "POS 1",
                IsActive = true,
                InvoiceSequence = 10
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EstablishmentId = establishmentId,
                Establishment = establishment,
                EmissionPointCode = "002",
                Name = "POS 2",
                IsActive = false,
                InvoiceSequence = 5
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EstablishmentId = establishmentId,
                Establishment = establishment,
                EmissionPointCode = "003",
                Name = "POS 3",
                IsActive = true,
                InvoiceSequence = 20
            }
        };

        _emissionPointRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emissionPoints);

        var query = new GetAllEmissionPointsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(3);
        result.Value.Should().Contain(ep => ep.EmissionPointCode == "001" && ep.Name == "POS 1");
        result.Value.Should().Contain(ep => ep.EmissionPointCode == "002" && ep.Name == "POS 2");
        result.Value.Should().Contain(ep => ep.EmissionPointCode == "003" && ep.Name == "POS 3");
        result.Value.Should().OnlyContain(ep => ep.EstablishmentCode == "001" && ep.EstablishmentName == "Main Office");

        _emissionPointRepositoryMock.Verify(r => r.GetAllAsync(
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoEmissionPoints_ShouldReturnEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var emissionPoints = new List<EmissionPoint>();

        _emissionPointRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emissionPoints);

        var query = new GetAllEmissionPointsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var query = new GetAllEmissionPointsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");

        _emissionPointRepositoryMock.Verify(r => r.GetAllAsync(
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_MixedActiveStatus_ShouldReturnAll()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var establishment = new Establishment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EstablishmentCode = "001",
            Name = "Main Office",
            Address = "123 Main St",
            IsActive = true
        };

        var emissionPoints = new List<EmissionPoint>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EstablishmentId = establishment.Id,
                Establishment = establishment,
                EmissionPointCode = "001",
                Name = "Active POS",
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EstablishmentId = establishment.Id,
                Establishment = establishment,
                EmissionPointCode = "002",
                Name = "Inactive POS",
                IsActive = false
            }
        };

        _emissionPointRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emissionPoints);

        var query = new GetAllEmissionPointsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
        result.Value.Should().Contain(ep => ep.IsActive == true);
        result.Value.Should().Contain(ep => ep.IsActive == false);
    }

    [Fact]
    public async Task Handle_SequenceNumbers_ShouldMapCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var establishment = new Establishment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EstablishmentCode = "001",
            Name = "Main Office",
            Address = "123 Main St",
            IsActive = true
        };

        var emissionPoints = new List<EmissionPoint>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EstablishmentId = establishment.Id,
                Establishment = establishment,
                EmissionPointCode = "001",
                Name = "Test POS",
                IsActive = true,
                InvoiceSequence = 100,
                CreditNoteSequence = 50,
                DebitNoteSequence = 25,
                RetentionSequence = 10
            }
        };

        _emissionPointRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emissionPoints);

        var query = new GetAllEmissionPointsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var emissionPoint = result.Value!.First();
        emissionPoint.InvoiceSequence.Should().Be(100);
        emissionPoint.CreditNoteSequence.Should().Be(50);
        emissionPoint.DebitNoteSequence.Should().Be(25);
        emissionPoint.RetentionSequence.Should().Be(10);
    }
}
