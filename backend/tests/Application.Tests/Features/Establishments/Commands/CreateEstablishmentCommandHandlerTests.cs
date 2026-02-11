using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.DTOs;
using SaaS.Application.Features.Establishments.Commands.CreateEstablishment;
using SaaS.Domain.Entities;
using Xunit;

namespace Application.Tests.Features.Establishments.Commands;

public class CreateEstablishmentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<CreateEstablishmentCommandHandler>> _loggerMock;
    private readonly Mock<IEstablishmentRepository> _establishmentRepositoryMock;
    private readonly CreateEstablishmentCommandHandler _handler;

    public CreateEstablishmentCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<CreateEstablishmentCommandHandler>>();
        _establishmentRepositoryMock = new Mock<IEstablishmentRepository>();

        _unitOfWorkMock.Setup(u => u.Establishments).Returns(_establishmentRepositoryMock.Object);

        _handler = new CreateEstablishmentCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateEstablishment()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var command = new CreateEstablishmentCommand
        {
            EstablishmentCode = "001",
            Name = "Main Office",
            Address = "123 Main St, Quito",
            Phone = "+593-2-1234567",
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.EstablishmentCode.Should().Be("001");
        result.Value.Name.Should().Be("Main Office");
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.IsActive.Should().BeTrue();

        _establishmentRepositoryMock.Verify(r => r.AddAsync(
            It.Is<Establishment>(e =>
                e.EstablishmentCode == "001" &&
                e.Name == "Main Office" &&
                e.TenantId == tenantId &&
                e.IsActive == true),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new CreateEstablishmentCommand
        {
            EstablishmentCode = "001",
            Name = "Main Office",
            Address = "123 Main St",
            Phone = "+593-2-1234567"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");

        _establishmentRepositoryMock.Verify(r => r.AddAsync(
            It.IsAny<Establishment>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_OptionalFieldsNull_ShouldCreateWithNullValues()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var command = new CreateEstablishmentCommand
        {
            EstablishmentCode = "002",
            Name = "Branch Office",
            Address = "456 Branch Ave",
            Phone = null,  // Optional field
            IsActive = false
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Phone.Should().BeNull();
        result.Value.IsActive.Should().BeFalse();

        _establishmentRepositoryMock.Verify(r => r.AddAsync(
            It.Is<Establishment>(e => e.Phone == null && e.IsActive == false),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ThreeDigitCode_ShouldCreateSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var command = new CreateEstablishmentCommand
        {
            EstablishmentCode = "123",
            Name = "Test Establishment",
            Address = "Test Address",
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.EstablishmentCode.Should().MatchRegex(@"^\d{3}$");
    }
}
