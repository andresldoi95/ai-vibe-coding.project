using Xunit;
using FluentAssertions;
using Moq;
using SaaS.Application.Features.Roles.Commands.DeleteRole;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.Roles.Commands;

public class DeleteRoleCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly DeleteRoleCommandHandler _handler;

    public DeleteRoleCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _roleRepositoryMock = new Mock<IRoleRepository>();

        _unitOfWorkMock.Setup(u => u.Roles).Returns(_roleRepositoryMock.Object);

        _handler = new DeleteRoleCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRole_ShouldSoftDeleteRole()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var role = new Role
        {
            Id = roleId,
            TenantId = tenantId,
            Name = "Manager",
            IsSystemRole = false,
            IsDeleted = false,
            RolePermissions = new List<RolePermission>()
        };

        _roleRepositoryMock
            .Setup(r => r.GetByIdWithPermissionsAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _roleRepositoryMock
            .Setup(r => r.GetUserCountAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0); // No users assigned

        var command = new DeleteRoleCommand { Id = roleId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        role.IsDeleted.Should().BeTrue();
        role.DeletedAt.Should().NotBeNull();
        role.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _roleRepositoryMock.Verify(r => r.UpdateAsync(role, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SystemRole_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var systemRole = new Role
        {
            Id = roleId,
            TenantId = tenantId,
            Name = "Admin",
            IsSystemRole = true,
            RolePermissions = new List<RolePermission>()
        };

        _roleRepositoryMock
            .Setup(r => r.GetByIdWithPermissionsAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(systemRole);

        var command = new DeleteRoleCommand { Id = roleId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("System roles cannot be deleted");

        _roleRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RoleWithUsers_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var role = new Role
        {
            Id = roleId,
            TenantId = tenantId,
            Name = "Manager",
            IsSystemRole = false,
            RolePermissions = new List<RolePermission>()
        };

        _roleRepositoryMock
            .Setup(r => r.GetByIdWithPermissionsAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _roleRepositoryMock
            .Setup(r => r.GetUserCountAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5); // 5 users assigned

        var command = new DeleteRoleCommand { Id = roleId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Cannot delete role");
        result.Error.Should().Contain("5 user(s) are assigned");

        role.IsDeleted.Should().BeFalse();
        _roleRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RoleNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _roleRepositoryMock
            .Setup(r => r.GetByIdWithPermissionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        var command = new DeleteRoleCommand { Id = Guid.NewGuid() };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Role not found");
    }

    [Fact]
    public async Task Handle_DifferentTenant_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var role = new Role
        {
            Id = roleId,
            TenantId = otherTenantId, // Different tenant
            Name = "Manager",
            IsSystemRole = false,
            RolePermissions = new List<RolePermission>()
        };

        _roleRepositoryMock
            .Setup(r => r.GetByIdWithPermissionsAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        var command = new DeleteRoleCommand { Id = roleId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Role not found");
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new DeleteRoleCommand { Id = Guid.NewGuid() };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context not set");
    }

    [Fact]
    public async Task Handle_MultipleUsers_ShouldShowCorrectCount()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var role = new Role
        {
            Id = roleId,
            TenantId = tenantId,
            Name = "Manager",
            IsSystemRole = false,
            RolePermissions = new List<RolePermission>()
        };

        _roleRepositoryMock
            .Setup(r => r.GetByIdWithPermissionsAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _roleRepositoryMock
            .Setup(r => r.GetUserCountAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(25); // 25 users

        var command = new DeleteRoleCommand { Id = roleId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("25 user(s)");
    }
}
