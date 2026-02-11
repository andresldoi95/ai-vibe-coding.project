using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Features.Establishments.Commands.DeleteEstablishment;
using SaaS.Domain.Entities;
using Xunit;

namespace Application.Tests.Features.Establishments.Commands;

public class DeleteEstablishmentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<DeleteEstablishmentCommandHandler>> _loggerMock;
    private readonly Mock<IEstablishmentRepository> _establishmentRepositoryMock;
    private readonly DeleteEstablishmentCommandHandler _handler;

    public DeleteEstablishmentCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<DeleteEstablishmentCommandHandler>>();
        _establishmentRepositoryMock = new Mock<IEstablishmentRepository>();

        _unitOfWorkMock.Setup(u => u.Establishments).Returns(_establishmentRepositoryMock.Object);

        _handler = new DeleteEstablishmentCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldDeleteEstablishment()
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
            Name = "Test Establishment",
            Address = "Test Address"
        };

        _establishmentRepositoryMock
            .Setup(r => r.GetByIdAsync(establishmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(establishment);

        var command = new DeleteEstablishmentCommand(establishmentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);

        _establishmentRepositoryMock.Verify(r => r.DeleteAsync(
            It.Is<Establishment>(e => e.Id == establishmentId),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new DeleteEstablishmentCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");

        _establishmentRepositoryMock.Verify(r => r.DeleteAsync(
            It.IsAny<Establishment>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_EstablishmentNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var establishmentId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _establishmentRepositoryMock
            .Setup(r => r.GetByIdAsync(establishmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Establishment?)null);

        var command = new DeleteEstablishmentCommand(establishmentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");

        _establishmentRepositoryMock.Verify(r => r.DeleteAsync(
            It.IsAny<Establishment>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_EstablishmentBelongsToAnotherTenant_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var differentTenantId = Guid.NewGuid();
        var establishmentId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var establishment = new Establishment
        {
            Id = establishmentId,
            TenantId = differentTenantId,  // Different tenant!
            EstablishmentCode = "001",
            Name = "Test",
            Address = "Test Address"
        };

        _establishmentRepositoryMock
            .Setup(r => r.GetByIdAsync(establishmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(establishment);

        var command = new DeleteEstablishmentCommand(establishmentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");

        _establishmentRepositoryMock.Verify(r => r.DeleteAsync(
            It.IsAny<Establishment>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
