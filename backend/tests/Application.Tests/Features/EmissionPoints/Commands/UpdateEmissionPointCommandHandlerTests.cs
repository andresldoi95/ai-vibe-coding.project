using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.DTOs;
using SaaS.Application.Features.EmissionPoints.Commands.UpdateEmissionPoint;
using SaaS.Domain.Entities;
using Xunit;

namespace Application.Tests.Features.EmissionPoints.Commands;

public class UpdateEmissionPointCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<UpdateEmissionPointCommandHandler>> _loggerMock;
    private readonly Mock<IEmissionPointRepository> _emissionPointRepositoryMock;
    private readonly Mock<IEstablishmentRepository> _establishmentRepositoryMock;
    private readonly UpdateEmissionPointCommandHandler _handler;

    public UpdateEmissionPointCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<UpdateEmissionPointCommandHandler>>();
        _emissionPointRepositoryMock = new Mock<IEmissionPointRepository>();
        _establishmentRepositoryMock = new Mock<IEstablishmentRepository>();

        _unitOfWorkMock.Setup(u => u.EmissionPoints).Returns(_emissionPointRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Establishments).Returns(_establishmentRepositoryMock.Object);

        _handler = new UpdateEmissionPointCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateEmissionPoint()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var establishmentId = Guid.NewGuid();
        var emissionPointId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var establishment = new Establishment
        {
            Id = establishmentId,
            TenantId = tenantId,
            EstablishmentCode = "001",
            Name = "Main Office",
            Address = "Test"
        };

        var existingEmissionPoint = new EmissionPoint
        {
            Id = emissionPointId,
            TenantId = tenantId,
            EstablishmentId = establishmentId,
            EmissionPointCode = "001",
            Name = "Old Name",
            IsActive = true,
            InvoiceSequence = 5,
            CreditNoteSequence = 3
        };

        _emissionPointRepositoryMock
            .Setup(r => r.GetByIdAsync(emissionPointId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEmissionPoint);

        _establishmentRepositoryMock
            .Setup(r => r.GetByIdAsync(establishmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(establishment);

        var command = new UpdateEmissionPointCommand
        {
            Id = emissionPointId,
            EstablishmentId = establishmentId,
            EmissionPointCode = "002",
            Name = "Updated Name",
            IsActive = false
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.EmissionPointCode.Should().Be("002");
        result.Value.Name.Should().Be("Updated Name");
        result.Value.EstablishmentCode.Should().Be("001");
        result.Value.EstablishmentName.Should().Be("Main Office");
        result.Value.IsActive.Should().BeFalse();
        result.Value.InvoiceSequence.Should().Be(5, "Sequences should not change on update");

        _emissionPointRepositoryMock.Verify(r => r.UpdateAsync(
            It.Is<EmissionPoint>(ep =>
                ep.EmissionPointCode == "002" &&
                ep.Name == "Updated Name" &&
                ep.IsActive == false),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new UpdateEmissionPointCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentId = Guid.NewGuid(),
            EmissionPointCode = "001",
            Name = "Test"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");

        _emissionPointRepositoryMock.Verify(r => r.UpdateAsync(
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

        var command = new UpdateEmissionPointCommand
        {
            Id = emissionPointId,
            EstablishmentId = Guid.NewGuid(),
            EmissionPointCode = "001",
            Name = "Test"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");

        _emissionPointRepositoryMock.Verify(r => r.UpdateAsync(
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

        var command = new UpdateEmissionPointCommand
        {
            Id = emissionPointId,
            EstablishmentId = Guid.NewGuid(),
            EmissionPointCode = "001",
            Name = "Updated"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");

        _emissionPointRepositoryMock.Verify(r => r.UpdateAsync(
            It.IsAny<EmissionPoint>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_EstablishmentNotFound_ShouldReturnFailure()
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
            EstablishmentId = Guid.NewGuid(),
            EmissionPointCode = "001",
            Name = "Test"
        };

        _emissionPointRepositoryMock
            .Setup(r => r.GetByIdAsync(emissionPointId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emissionPoint);

        _establishmentRepositoryMock
            .Setup(r => r.GetByIdAsync(establishmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Establishment?)null);

        var command = new UpdateEmissionPointCommand
        {
            Id = emissionPointId,
            EstablishmentId = establishmentId,
            EmissionPointCode = "001",
            Name = "Updated"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Establishment not found");
    }
}
