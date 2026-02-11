using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.DTOs;
using SaaS.Application.Features.Establishments.Queries.GetEstablishmentById;
using SaaS.Domain.Entities;
using Xunit;

namespace Application.Tests.Features.Establishments.Queries;

public class GetEstablishmentByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetEstablishmentByIdQueryHandler>> _loggerMock;
    private readonly Mock<IEstablishmentRepository> _establishmentRepositoryMock;
    private readonly GetEstablishmentByIdQueryHandler _handler;

    public GetEstablishmentByIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetEstablishmentByIdQueryHandler>>();
        _establishmentRepositoryMock = new Mock<IEstablishmentRepository>();

        _unitOfWorkMock.Setup(u => u.Establishments).Returns(_establishmentRepositoryMock.Object);

        _handler = new GetEstablishmentByIdQueryHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnEstablishment()
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
            Phone = "+593-2-1234567",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow
        };

        _establishmentRepositoryMock
            .Setup(r => r.GetByIdAsync(establishmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(establishment);

        var query = new GetEstablishmentByIdQuery(establishmentId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(establishmentId);
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.EstablishmentCode.Should().Be("001");
        result.Value.Name.Should().Be("Main Office");
        result.Value.Address.Should().Be("123 Main St");
        result.Value.Phone.Should().Be("+593-2-1234567");
        result.Value.IsActive.Should().BeTrue();

        _establishmentRepositoryMock.Verify(r => r.GetByIdAsync(
            establishmentId,
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var query = new GetEstablishmentByIdQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");

        _establishmentRepositoryMock.Verify(r => r.GetByIdAsync(
            It.IsAny<Guid>(),
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

        var query = new GetEstablishmentByIdQuery(establishmentId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
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

        var query = new GetEstablishmentByIdQuery(establishmentId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_EstablishmentWithNullPhone_ShouldReturnEstablishment()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var establishmentId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var establishment = new Establishment
        {
            Id = establishmentId,
            TenantId = tenantId,
            EstablishmentCode = "002",
            Name = "Branch Office",
            Address = "456 Branch Ave",
            Phone = null,  // Nullable field
            IsActive = true
        };

        _establishmentRepositoryMock
            .Setup(r => r.GetByIdAsync(establishmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(establishment);

        var query = new GetEstablishmentByIdQuery(establishmentId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Phone.Should().BeNull();
    }
}
