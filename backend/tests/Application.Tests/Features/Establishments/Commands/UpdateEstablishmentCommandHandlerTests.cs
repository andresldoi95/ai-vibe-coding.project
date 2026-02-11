using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.DTOs;
using SaaS.Application.Features.Establishments.Commands.UpdateEstablishment;
using SaaS.Domain.Entities;
using Xunit;

namespace Application.Tests.Features.Establishments.Commands;

public class UpdateEstablishmentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<UpdateEstablishmentCommandHandler>> _loggerMock;
    private readonly Mock<IEstablishmentRepository> _establishmentRepositoryMock;
    private readonly UpdateEstablishmentCommandHandler _handler;

    public UpdateEstablishmentCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<UpdateEstablishmentCommandHandler>>();
        _establishmentRepositoryMock = new Mock<IEstablishmentRepository>();

        _unitOfWorkMock.Setup(u => u.Establishments).Returns(_establishmentRepositoryMock.Object);

        _handler = new UpdateEstablishmentCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateEstablishment()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var establishmentId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var existingEstablishment = new Establishment
        {
            Id = establishmentId,
            TenantId = tenantId,
            EstablishmentCode = "001",
            Name = "Old Name",
            Address = "Old Address",
            Phone = "Old Phone",
            IsActive = true
        };

        _establishmentRepositoryMock
            .Setup(r => r.GetByIdAsync(establishmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEstablishment);

        var command = new UpdateEstablishmentCommand
        {
            Id = establishmentId,
            EstablishmentCode = "002",
            Name = "Updated Name",
            Address = "Updated Address",
            Phone = "+593-2-9999999",
            IsActive = false
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.EstablishmentCode.Should().Be("002");
        result.Value.Name.Should().Be("Updated Name");
        result.Value.Address.Should().Be("Updated Address");
        result.Value.Phone.Should().Be("+593-2-9999999");
        result.Value.IsActive.Should().BeFalse();

        _establishmentRepositoryMock.Verify(r => r.UpdateAsync(
            It.Is<Establishment>(e =>
                e.EstablishmentCode == "002" &&
                e.Name == "Updated Name" &&
                e.IsActive == false),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new UpdateEstablishmentCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentCode = "001",
            Name = "Test",
            Address = "Test Address"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");

        _establishmentRepositoryMock.Verify(r => r.UpdateAsync(
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

        var command = new UpdateEstablishmentCommand
        {
            Id = establishmentId,
            EstablishmentCode = "001",
            Name = "Test",
            Address = "Test Address"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");

        _establishmentRepositoryMock.Verify(r => r.UpdateAsync(
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

        var command = new UpdateEstablishmentCommand
        {
            Id = establishmentId,
            EstablishmentCode = "001",
            Name = "Updated",
            Address = "Updated Address"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");

        _establishmentRepositoryMock.Verify(r => r.UpdateAsync(
            It.IsAny<Establishment>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_NullablePhone_ShouldUpdateSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var establishmentId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var existingEstablishment = new Establishment
        {
            Id = establishmentId,
            TenantId = tenantId,
            EstablishmentCode = "001",
            Name = "Test",
            Address = "Test Address",
            Phone = "12345"
        };

        _establishmentRepositoryMock
            .Setup(r => r.GetByIdAsync(establishmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEstablishment);

        var command = new UpdateEstablishmentCommand
        {
            Id = establishmentId,
            EstablishmentCode = "001",
            Name = "Test",
            Address = "Test Address",
            Phone = null,  // Remove phone number
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Phone.Should().BeNull();
    }
}
