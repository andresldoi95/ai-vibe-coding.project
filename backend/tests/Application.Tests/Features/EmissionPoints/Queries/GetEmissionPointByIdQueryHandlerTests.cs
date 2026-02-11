using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.DTOs;
using SaaS.Application.Features.EmissionPoints.Queries.GetEmissionPointById;
using SaaS.Domain.Entities;
using Xunit;

namespace Application.Tests.Features.EmissionPoints.Queries;

public class GetEmissionPointByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetEmissionPointByIdQueryHandler>> _loggerMock;
    private readonly Mock<IEmissionPointRepository> _emissionPointRepositoryMock;
    private readonly GetEmissionPointByIdQueryHandler _handler;

    public GetEmissionPointByIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetEmissionPointByIdQueryHandler>>();
        _emissionPointRepositoryMock = new Mock<IEmissionPointRepository>();

        _unitOfWorkMock.Setup(u => u.EmissionPoints).Returns(_emissionPointRepositoryMock.Object);

        _handler = new GetEmissionPointByIdQueryHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnEmissionPoint()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var emissionPointId = Guid.NewGuid();
        var establishmentId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var emissionPoint = new EmissionPoint
        {
            Id = emissionPointId,
            TenantId = tenantId,
            EstablishmentId = establishmentId,
            EmissionPointCode = "001",
            Name = "POS 1",
            IsActive = true,
            InvoiceSequence = 10,
            CreditNoteSequence = 5,
            DebitNoteSequence = 3,
            RetentionSequence = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow
        };

        _emissionPointRepositoryMock
            .Setup(r => r.GetByIdAsync(emissionPointId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emissionPoint);

        var query = new GetEmissionPointByIdQuery(emissionPointId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(emissionPointId);
        result.Value.EstablishmentId.Should().Be(establishmentId);
        result.Value.EmissionPointCode.Should().Be("001");
        result.Value.Name.Should().Be("POS 1");
        result.Value.IsActive.Should().BeTrue();
        result.Value.InvoiceSequence.Should().Be(10);
        result.Value.CreditNoteSequence.Should().Be(5);
        result.Value.DebitNoteSequence.Should().Be(3);
        result.Value.RetentionSequence.Should().Be(1);

        _emissionPointRepositoryMock.Verify(r => r.GetByIdAsync(
            emissionPointId,
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var query = new GetEmissionPointByIdQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");

        _emissionPointRepositoryMock.Verify(r => r.GetByIdAsync(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_EmissionPointNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var emissionPointId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _emissionPointRepositoryMock
            .Setup(r => r.GetByIdAsync(emissionPointId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmissionPoint?)null);

        var query = new GetEmissionPointByIdQuery(emissionPointId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_EmissionPointBelongsToAnotherTenant_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var differentTenantId = Guid.NewGuid();
        var emissionPointId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var emissionPoint = new EmissionPoint
        {
            Id = emissionPointId,
            TenantId = differentTenantId,  // Different tenant!
            EstablishmentId = Guid.NewGuid(),
            EmissionPointCode = "001",
            Name = "Test"
        };

        _emissionPointRepositoryMock
            .Setup(r => r.GetByIdAsync(emissionPointId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emissionPoint);

        var query = new GetEmissionPointByIdQuery(emissionPointId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_EmissionPointWithAllSequences_ShouldMapCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var emissionPointId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var emissionPoint = new EmissionPoint
        {
            Id = emissionPointId,
            TenantId = tenantId,
            EstablishmentId = Guid.NewGuid(),
            EmissionPointCode = "999",
            Name = "High Volume POS",
            IsActive = true,
            InvoiceSequence = 1000,
            CreditNoteSequence = 250,
            DebitNoteSequence = 150,
            RetentionSequence = 75
        };

        _emissionPointRepositoryMock
            .Setup(r => r.GetByIdAsync(emissionPointId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emissionPoint);

        var query = new GetEmissionPointByIdQuery(emissionPointId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.InvoiceSequence.Should().Be(1000);
        result.Value.CreditNoteSequence.Should().Be(250);
        result.Value.DebitNoteSequence.Should().Be(150);
        result.Value.RetentionSequence.Should().Be(75);
    }
}
