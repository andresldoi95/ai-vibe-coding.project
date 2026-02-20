using Xunit;
using FluentAssertions;
using Moq;
using SaaS.Application.Features.Users.Commands.UpdateUserRole;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Features.Users.Commands;

public class UpdateUserRoleCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IUserTenantRepository> _userTenantRepositoryMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<ILogger<UpdateUserRoleCommandHandler>> _loggerMock;
    private readonly UpdateUserRoleCommandHandler _handler;

    public UpdateUserRoleCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _userTenantRepositoryMock = new Mock<IUserTenantRepository>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _loggerMock = new Mock<ILogger<UpdateUserRoleCommandHandler>>();

        _unitOfWorkMock.Setup(u => u.UserTenants).Returns(_userTenantRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Roles).Returns(_roleRepositoryMock.Object);

        _handler = new UpdateUserRoleCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRoleUpdate_ShouldSucceed()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var oldRoleId = Guid.NewGuid();
        var newRoleId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var oldRole = new Role { Id = oldRoleId, TenantId = tenantId, Name = "Manager", IsSystemRole = false };
        var newRole = new Role { Id = newRoleId, TenantId = tenantId, Name = "Admin", IsSystemRole = false };

        var userTenant = new UserTenant
        {
            UserId = userId,
            TenantId = tenantId,
            RoleId = oldRoleId,
            Role = oldRole,
            IsActive = true
        };

        _userTenantRepositoryMock
            .Setup(r => r.GetByUserAndTenantAsync(userId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userTenant);

        _roleRepositoryMock
            .Setup(r => r.GetByIdAsync(newRoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newRole);

        var command = new UpdateUserRoleCommand
        {
            UserId = userId,
            NewRoleId = newRoleId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        userTenant.RoleId.Should().Be(newRoleId);
        _userTenantRepositoryMock.Verify(r => r.UpdateAsync(userTenant, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new UpdateUserRoleCommand
        {
            UserId = Guid.NewGuid(),
            NewRoleId = Guid.NewGuid()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

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

        var command = new UpdateUserRoleCommand
        {
            UserId = userId,
            NewRoleId = Guid.NewGuid()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User not found in this company");
    }

    [Fact]
    public async Task Handle_InactiveUser_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var userTenant = new UserTenant
        {
            UserId = userId,
            TenantId = tenantId,
            IsActive = false // Inactive
        };

        _userTenantRepositoryMock
            .Setup(r => r.GetByUserAndTenantAsync(userId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userTenant);

        var command = new UpdateUserRoleCommand
        {
            UserId = userId,
            NewRoleId = Guid.NewGuid()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User not found in this company");
    }

    [Fact]
    public async Task Handle_InvalidRole_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var newRoleId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var userTenant = new UserTenant
        {
            UserId = userId,
            TenantId = tenantId,
            Role = new Role { Name = "Manager" },
            IsActive = true
        };

        _userTenantRepositoryMock
            .Setup(r => r.GetByUserAndTenantAsync(userId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userTenant);

        _roleRepositoryMock
            .Setup(r => r.GetByIdAsync(newRoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        var command = new UpdateUserRoleCommand
        {
            UserId = userId,
            NewRoleId = newRoleId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid role selected");
    }

    [Fact]
    public async Task Handle_RoleFromDifferentTenant_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var newRoleId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var userTenant = new UserTenant
        {
            UserId = userId,
            TenantId = tenantId,
            Role = new Role { Name = "Manager" },
            IsActive = true
        };

        var newRole = new Role { Id = newRoleId, TenantId = otherTenantId, Name = "Admin" }; // Different tenant

        _userTenantRepositoryMock
            .Setup(r => r.GetByUserAndTenantAsync(userId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userTenant);

        _roleRepositoryMock
            .Setup(r => r.GetByIdAsync(newRoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newRole);

        var command = new UpdateUserRoleCommand
        {
            UserId = userId,
            NewRoleId = newRoleId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid role selected");
    }

    [Fact]
    public async Task Handle_DemotingLastOwner_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ownerRoleId = Guid.NewGuid();
        var managerRoleId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var ownerRole = new Role { Id = ownerRoleId, TenantId = tenantId, Name = "Owner", IsSystemRole = true };
        var managerRole = new Role { Id = managerRoleId, TenantId = tenantId, Name = "Manager", IsSystemRole = false };

        var userTenant = new UserTenant
        {
            UserId = userId,
            TenantId = tenantId,
            RoleId = ownerRoleId,
            Role = ownerRole,
            IsActive = true
        };

        _userTenantRepositoryMock
            .Setup(r => r.GetByUserAndTenantAsync(userId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userTenant);

        _roleRepositoryMock
            .Setup(r => r.GetByIdAsync(managerRoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(managerRole);

        _roleRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role> { ownerRole, managerRole });

        _userTenantRepositoryMock
            .Setup(r => r.GetByTenantIdWithDetailsAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserTenant> { userTenant }); // Only one owner

        var command = new UpdateUserRoleCommand
        {
            UserId = userId,
            NewRoleId = managerRoleId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Cannot change the role of the last owner");
        _userTenantRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UserTenant>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DemotingOwnerWithOtherOwnersPresent_ShouldSucceed()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        var ownerRoleId = Guid.NewGuid();
        var managerRoleId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var ownerRole = new Role { Id = ownerRoleId, TenantId = tenantId, Name = "Owner", IsSystemRole = true };
        var managerRole = new Role { Id = managerRoleId, TenantId = tenantId, Name = "Manager", IsSystemRole = false };

        var userTenant1 = new UserTenant
        {
            UserId = user1Id,
            TenantId = tenantId,
            RoleId = ownerRoleId,
            Role = ownerRole,
            IsActive = true
        };

        var userTenant2 = new UserTenant
        {
            UserId = user2Id,
            TenantId = tenantId,
            RoleId = ownerRoleId,
            Role = ownerRole,
            IsActive = true
        };

        _userTenantRepositoryMock
            .Setup(r => r.GetByUserAndTenantAsync(user1Id, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userTenant1);

        _roleRepositoryMock
            .Setup(r => r.GetByIdAsync(managerRoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(managerRole);

        _roleRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role> { ownerRole, managerRole });

        _userTenantRepositoryMock
            .Setup(r => r.GetByTenantIdWithDetailsAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserTenant> { userTenant1, userTenant2 }); // Two owners

        var command = new UpdateUserRoleCommand
        {
            UserId = user1Id,
            NewRoleId = managerRoleId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        userTenant1.RoleId.Should().Be(managerRoleId);
        _userTenantRepositoryMock.Verify(r => r.UpdateAsync(userTenant1, It.IsAny<CancellationToken>()), Times.Once);
    }
}
