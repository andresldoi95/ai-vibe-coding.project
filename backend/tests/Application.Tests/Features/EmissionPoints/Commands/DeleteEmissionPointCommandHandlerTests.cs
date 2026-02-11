using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Features.EmissionPoints.Commands.DeleteEmissionPoint;
using SaaS.Domain.Entities;
using Xunit;

namespace Application.Tests.Features.EmissionPoints.Commands;

public class DeleteEmissionPointCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<DeleteEmissionPointCommandHandler>> _loggerMock;
    private readonly Mock<IEmissionPointRepository> _emissionPointRepositoryMock;
    private readonly DeleteEmissionPointCommandHandler _handler;

    public DeleteEmissionPointCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<DeleteEmissionPointCommandHandler>>();
        _emissionPointRepositoryMock = new Mock<IEmissionPointRepository>();

        _unitOfWorkMock.Setup(u => u.EmissionPoints).Returns(_emissionPointRepositoryMock.Object);

        _handler = new DeleteEmissionPointCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldDeleteEmissionPoint()
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
            EmissionPointCode = "001",
            Name = "POS 1"
        };

        _emissionPointRepositoryMock
            .Setup(r => r.GetByIdAsync(emissionPointId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emissionPoint);

        var command = new DeleteEmissionPointCommand(emissionPointId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);

        _emissionPointRepositoryMock.Verify(r => r.DeleteAsync(
            It.Is<EmissionPoint>(ep => ep.Id == emissionPointId),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new DeleteEmissionPointCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");

        _emissionPointRepositoryMock.Verify(r => r.DeleteAsync(
            It.IsAny<EmissionPoint>(),
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

        var command = new DeleteEmissionPointCommand(emissionPointId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");

        _emissionPointRepositoryMock.Verify(r => r.DeleteAsync(
            It.IsAny<EmissionPoint>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
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

        var command = new DeleteEmissionPointCommand(emissionPointId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");

        _emissionPointRepositoryMock.Verify(r => r.DeleteAsync(
            It.IsAny<EmissionPoint>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
