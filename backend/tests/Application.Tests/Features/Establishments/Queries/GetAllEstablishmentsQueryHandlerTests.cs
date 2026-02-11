using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.DTOs;
using SaaS.Application.Features.Establishments.Queries.GetAllEstablishments;
using SaaS.Domain.Entities;
using Xunit;

namespace Application.Tests.Features.Establishments.Queries;

public class GetAllEstablishmentsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetAllEstablishmentsQueryHandler>> _loggerMock;
    private readonly Mock<IEstablishmentRepository> _establishmentRepositoryMock;
    private readonly GetAllEstablishmentsQueryHandler _handler;

    public GetAllEstablishmentsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetAllEstablishmentsQueryHandler>>();
        _establishmentRepositoryMock = new Mock<IEstablishmentRepository>();

        _unitOfWorkMock.Setup(u => u.Establishments).Returns(_establishmentRepositoryMock.Object);

        _handler = new GetAllEstablishmentsQueryHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnAllEstablishments()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var establishments = new List<Establishment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EstablishmentCode = "001",
                Name = "Main Office",
                Address = "123 Main St",
                Phone = "+593-2-1234567",
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EstablishmentCode = "002",
                Name = "Branch Office",
                Address = "456 Branch Ave",
                Phone = null,
                IsActive = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EstablishmentCode = "003",
                Name = "Warehouse",
                Address = "789 Storage Blvd",
                Phone = "+593-2-9999999",
                IsActive = true
            }
        };

        _establishmentRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(establishments);

        var query = new GetAllEstablishmentsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(3);
        result.Value.Should().Contain(e => e.EstablishmentCode == "001" && e.Name == "Main Office");
        result.Value.Should().Contain(e => e.EstablishmentCode == "002" && e.Name == "Branch Office");
        result.Value.Should().Contain(e => e.EstablishmentCode == "003" && e.Name == "Warehouse");

        _establishmentRepositoryMock.Verify(r => r.GetAllAsync(
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoEstablishments_ShouldReturnEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var establishments = new List<Establishment>();

        _establishmentRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(establishments);

        var query = new GetAllEstablishmentsQuery();

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

        var query = new GetAllEstablishmentsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");

        _establishmentRepositoryMock.Verify(r => r.GetAllAsync(
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_MixedActiveStatus_ShouldReturnAll()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var establishments = new List<Establishment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EstablishmentCode = "001",
                Name = "Active Establishment",
                Address = "Address 1",
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EstablishmentCode = "002",
                Name = "Inactive Establishment",
                Address = "Address 2",
                IsActive = false
            }
        };

        _establishmentRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(establishments);

        var query = new GetAllEstablishmentsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
        result.Value.Should().Contain(e => e.IsActive == true);
        result.Value.Should().Contain(e => e.IsActive == false);
    }

    [Fact]
    public async Task Handle_EstablishmentsWithNullableFields_ShouldMapCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var establishments = new List<Establishment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EstablishmentCode = "001",
                Name = "Test",
                Address = "Test Address",
                Phone = null,  // Null phone
                IsActive = true
            }
        };

        _establishmentRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(establishments);

        var query = new GetAllEstablishmentsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.First().Phone.Should().BeNull();
    }
}
