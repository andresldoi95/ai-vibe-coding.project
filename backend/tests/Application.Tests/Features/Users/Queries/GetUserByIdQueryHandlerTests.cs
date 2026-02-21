using Xunit;
using FluentAssertions;
using Moq;
using SaaS.Application.Features.Users.Queries.GetUserById;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Features.Users.Queries;

public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IUserTenantRepository> _userTenantRepositoryMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetUserByIdQueryHandler>> _loggerMock;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _userTenantRepositoryMock = new Mock<IUserTenantRepository>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetUserByIdQueryHandler>>();

        _handler = new GetUserByIdQueryHandler(
            _userTenantRepositoryMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnUserWithRole()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            Name = "Test User",
            IsActive = true
        };

        var role = new Role
        {
            Id = roleId,
            TenantId = tenantId,
            Name = "Manager",
            Description = "Warehouse Manager",
            Priority = 100
        };

        var userTenant = new UserTenant
        {
            UserId = userId,
            TenantId = tenantId,
            RoleId = roleId,
            User = user,
            Role = role,
            IsActive = true,
            JoinedAt = DateTime.UtcNow.AddDays(-30)
        };

        _userTenantRepositoryMock
            .Setup(r => r.GetByUserAndTenantAsync(userId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userTenant);

        var query = new GetUserByIdQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(userId);
        result.Value.Email.Should().Be("user@example.com");
        result.Value.Name.Should().Be("Test User");
        result.Value.Role.Should().NotBeNull();
        result.Value.Role.Id.Should().Be(roleId);
        result.Value.Role.Name.Should().Be("Manager");
        result.Value.Role.Description.Should().Be("Warehouse Manager");
        result.Value.Role.Priority.Should().Be(100);
        result.Value.IsActive.Should().BeTrue();
        result.Value.JoinedAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(-30), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var query = new GetUserByIdQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");
    }

    [Fact]
    public async Task Handle_UserNotFoundInCompany_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _userTenantRepositoryMock
            .Setup(r => r.GetByUserAndTenantAsync(userId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserTenant?)null);

        var query = new GetUserByIdQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User not found in this company");
    }

    [Fact]
    public async Task Handle_InactiveUser_ShouldReturnUserWithIsActiveFalse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var user = new User
        {
            Id = userId,
            Email = "inactive@example.com",
            Name = "Inactive User",
            IsActive = false
        };

        var role = new Role
        {
            Id = roleId,
            TenantId = tenantId,
            Name = "Viewer",
            Description = "Read-only access",
            Priority = 200
        };

        var userTenant = new UserTenant
        {
            UserId = userId,
            TenantId = tenantId,
            RoleId = roleId,
            User = user,
            Role = role,
            IsActive = false, // Inactive in this company
            JoinedAt = DateTime.UtcNow.AddDays(-60)
        };

        _userTenantRepositoryMock
            .Setup(r => r.GetByUserAndTenantAsync(userId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userTenant);

        var query = new GetUserByIdQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(userId);
        result.Value.IsActive.Should().BeFalse();
    }
}
