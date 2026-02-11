using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.DTOs;
using SaaS.Application.Features.EmissionPoints.Commands.CreateEmissionPoint;
using SaaS.Domain.Entities;
using Xunit;

namespace Application.Tests.Features.EmissionPoints.Commands;

public class CreateEmissionPointCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<CreateEmissionPointCommandHandler>> _loggerMock;
    private readonly Mock<IEmissionPointRepository> _emissionPointRepositoryMock;
    private readonly Mock<IEstablishmentRepository> _establishmentRepositoryMock;
    private readonly CreateEmissionPointCommandHandler _handler;

    public CreateEmissionPointCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<CreateEmissionPointCommandHandler>>();
        _emissionPointRepositoryMock = new Mock<IEmissionPointRepository>();
        _establishmentRepositoryMock = new Mock<IEstablishmentRepository>();

        _unitOfWorkMock.Setup(u => u.EmissionPoints).Returns(_emissionPointRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Establishments).Returns(_establishmentRepositoryMock.Object);

        _handler = new CreateEmissionPointCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateEmissionPoint()
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
            Address = "Test Address"
        };

        _establishmentRepositoryMock
            .Setup(r => r.GetByIdAsync(establishmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(establishment);

        var command = new CreateEmissionPointCommand
        {
            EstablishmentId = establishmentId,
            EmissionPointCode = "001",
            Name = "POS 1",
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.EmissionPointCode.Should().Be("001");
        result.Value.Name.Should().Be("POS 1");
        result.Value.EstablishmentId.Should().Be(establishmentId);
        result.Value.EstablishmentCode.Should().Be("001");
        result.Value.EstablishmentName.Should().Be("Main Office");
        result.Value.IsActive.Should().BeTrue();
        result.Value.InvoiceSequence.Should().Be(1);
        result.Value.CreditNoteSequence.Should().Be(1);
        result.Value.DebitNoteSequence.Should().Be(1);
        result.Value.RetentionSequence.Should().Be(1);

        _emissionPointRepositoryMock.Verify(r => r.AddAsync(
            It.Is<EmissionPoint>(ep =>
                ep.EmissionPointCode == "001" &&
                ep.Name == "POS 1" &&
                ep.EstablishmentId == establishmentId &&
                ep.TenantId == tenantId &&
                ep.InvoiceSequence == 1 &&
                ep.CreditNoteSequence == 1 &&
                ep.DebitNoteSequence == 1 &&
                ep.RetentionSequence == 1),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new CreateEmissionPointCommand
        {
            EstablishmentId = Guid.NewGuid(),
            EmissionPointCode = "001",
            Name = "POS 1"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");

        _emissionPointRepositoryMock.Verify(r => r.AddAsync(
            It.IsAny<EmissionPoint>(),
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

        var command = new CreateEmissionPointCommand
        {
            EstablishmentId = establishmentId,
            EmissionPointCode = "001",
            Name = "POS 1"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Establishment not found");

        _emissionPointRepositoryMock.Verify(r => r.AddAsync(
            It.IsAny<EmissionPoint>(),
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

        var command = new CreateEmissionPointCommand
        {
            EstablishmentId = establishmentId,
            EmissionPointCode = "001",
            Name = "POS 1"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Establishment not found");

        _emissionPointRepositoryMock.Verify(r => r.AddAsync(
            It.IsAny<EmissionPoint>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_InactiveEmissionPoint_ShouldCreateSuccessfully()
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
            Address = "Test Address"
        };

        _establishmentRepositoryMock
            .Setup(r => r.GetByIdAsync(establishmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(establishment);

        var command = new CreateEmissionPointCommand
        {
            EstablishmentId = establishmentId,
            EmissionPointCode = "002",
            Name = "Inactive POS",
            IsActive = false
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.IsActive.Should().BeFalse();
        result.Value.InvoiceSequence.Should().Be(1);
    }

    [Fact]
    public async Task Handle_AllSequenceNumbers_ShouldInitializeToOne()
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
            Address = "Test Address"
        };

        _establishmentRepositoryMock
            .Setup(r => r.GetByIdAsync(establishmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(establishment);

        var command = new CreateEmissionPointCommand
        {
            EstablishmentId = establishmentId,
            EmissionPointCode = "001",
            Name = "POS 1",
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.InvoiceSequence.Should().Be(1, "Invoice sequence should initialize to 1");
        result.Value.CreditNoteSequence.Should().Be(1, "Credit note sequence should initialize to 1");
        result.Value.DebitNoteSequence.Should().Be(1, "Debit note sequence should initialize to 1");
        result.Value.RetentionSequence.Should().Be(1, "Retention sequence should initialize to 1");
    }
}
