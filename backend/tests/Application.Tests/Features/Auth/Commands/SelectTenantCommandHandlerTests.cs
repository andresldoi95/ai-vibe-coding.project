using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.Auth.Commands.SelectTenant;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.Auth.Commands;

public class SelectTenantCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<SelectTenantCommandHandler>> _loggerMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUserTenantRepository> _userTenantRepositoryMock;
    private readonly SelectTenantCommandHandler _handler;

    public SelectTenantCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<SelectTenantCommandHandler>>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _userTenantRepositoryMock = new Mock<IUserTenantRepository>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.UserTenants).Returns(_userTenantRepositoryMock.Object);

        _handler = new SelectTenantCommandHandler(
            _unitOfWorkMock.Object,
            _authServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldReturnTokenWithRoleAndPermissions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

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
            Name = "Admin",
            Description = "Administrator",
            Priority = 50,
            RolePermissions = new List<RolePermission>
            {
                new RolePermission
                {
                    Permission = new Permission
                    {
                        Name = "warehouses:read",
                        Resource = "warehouses",
                        Action = "read"
                    }
                },
                new RolePermission
                {
                    Permission = new Permission
                    {
                        Name = "products:write",
                        Resource = "products",
                        Action = "write"
                    }
                }
            }
        };

        var userTenant = new UserTenant
        {
            UserId = userId,
            TenantId = tenantId,
            RoleId = roleId,
            Role = role,
            IsActive = true
        };

        var command = new SelectTenantCommand
        {
            UserId = userId,
            TenantId = tenantId
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userTenantRepositoryMock
            .Setup(r => r.GetWithRoleAndPermissionsAsync(userId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userTenant);

        _authServiceMock
            .Setup(a => a.GenerateJwtTokenWithRole(user, tenantId, role, It.IsAny<List<string>>()))
            .Returns("tenant_scoped_jwt_token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AccessToken.Should().Be("tenant_scoped_jwt_token");
        result.Value.Role.Should().NotBeNull();
        result.Value.Role.Name.Should().Be("Admin");
        result.Value.Permissions.Should().HaveCount(2);
        result.Value.Permissions.Should().Contain("warehouses:read");
        result.Value.Permissions.Should().Contain("products:write");
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new SelectTenantCommand
        {
            UserId = Guid.NewGuid(),
            TenantId = Guid.NewGuid()
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User not found");
    }

    [Fact]
    public async Task Handle_UnauthorizedTenant_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            Name = "Test User",
            IsActive = true
        };

        var command = new SelectTenantCommand
        {
            UserId = userId,
            TenantId = Guid.NewGuid()
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userTenantRepositoryMock
            .Setup(r => r.GetWithRoleAndPermissionsAsync(userId, command.TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserTenant?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access denied to this tenant");
    }

    [Fact]
    public async Task Handle_NoRoleAssigned_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            Name = "Test User",
            IsActive = true
        };

        var userTenant = new UserTenant
        {
            UserId = userId,
            TenantId = tenantId,
            RoleId = null,
            Role = null,
            IsActive = true
        };

        var command = new SelectTenantCommand
        {
            UserId = userId,
            TenantId = tenantId
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userTenantRepositoryMock
            .Setup(r => r.GetWithRoleAndPermissionsAsync(userId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userTenant);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("No role assigned");
    }

    [Fact]
    public async Task Handle_ShouldLogInformationOnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

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
            Name = "Admin",
            Description = "Administrator",
            Priority = 50,
            RolePermissions = new List<RolePermission>()
        };

        var userTenant = new UserTenant
        {
            UserId = userId,
            TenantId = tenantId,
            RoleId = roleId,
            Role = role,
            IsActive = true
        };

        var command = new SelectTenantCommand
        {
            UserId = userId,
            TenantId = tenantId
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userTenantRepositoryMock
            .Setup(r => r.GetWithRoleAndPermissionsAsync(userId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userTenant);

        _authServiceMock
            .Setup(a => a.GenerateJwtTokenWithRole(user, tenantId, role, It.IsAny<List<string>>()))
            .Returns("tenant_scoped_jwt_token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("selected tenant")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
