using Xunit;
using FluentAssertions;
using Moq;
using SaaS.Application.Features.Roles.Commands.UpdateRole;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.Roles.Commands;

public class UpdateRoleCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly UpdateRoleCommandHandler _handler;

    public UpdateRoleCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _permissionRepositoryMock = new Mock<IPermissionRepository>();

        _unitOfWorkMock.Setup(u => u.Roles).Returns(_roleRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Permissions).Returns(_permissionRepositoryMock.Object);

        _handler = new UpdateRoleCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateRole()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var existingRole = new Role
        {
            Id = roleId,
            TenantId = tenantId,
            Name = "Old Name",
            Description = "Old Description",
            Priority = 50,
            IsSystemRole = false,
            RolePermissions = new List<RolePermission>()
        };

        _roleRepositoryMock
            .Setup(r => r.GetByIdWithPermissionsAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRole);

        _roleRepositoryMock
            .Setup(r => r.ExistsByNameAsync("New Name", tenantId, roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var permissions = new List<Permission>
        {
            new Permission { Id = permissionId, Name = "test.permission", Resource = "test", Action = "read" }
        };

        _permissionRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        _roleRepositoryMock
            .Setup(r => r.GetUserCountAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var command = new UpdateRoleCommand
        {
            Id = roleId,
            Name = "New Name",
            Description = "New Description",
            Priority = 100,
            PermissionIds = new List<Guid> { permissionId }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("New Name");
        result.Value.Description.Should().Be("New Description");
        result.Value.Priority.Should().Be(100);
        result.Value.UserCount.Should().Be(5);

        existingRole.Name.Should().Be("New Name");
        existingRole.Description.Should().Be("New Description");
        existingRole.Priority.Should().Be(100);

        _roleRepositoryMock.Verify(r => r.UpdateAsync(existingRole, It.IsAny<CancellationToken>()), Times.Once);
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

        var command = new UpdateRoleCommand
        {
            Id = roleId,
            Name = "Modified Admin",
            Description = "Try to modify",
            Priority = 100,
            PermissionIds = new List<Guid> { Guid.NewGuid() }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("System roles cannot be modified");

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

        var command = new UpdateRoleCommand
        {
            Id = Guid.NewGuid(),
            Name = "New Name",
            Description = "Description",
            Priority = 100,
            PermissionIds = new List<Guid> { Guid.NewGuid() }
        };

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
            TenantId = otherTenantId, // Different tenant!
            Name = "Manager",
            IsSystemRole = false,
            RolePermissions = new List<RolePermission>()
        };

        _roleRepositoryMock
            .Setup(r => r.GetByIdWithPermissionsAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        var command = new UpdateRoleCommand
        {
            Id = roleId,
            Name = "Modified Name",
            Description = "Description",
            Priority = 100,
            PermissionIds = new List<Guid> { Guid.NewGuid() }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Role not found");
    }

    [Fact]
    public async Task Handle_DuplicateRoleName_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var existingRole = new Role
        {
            Id = roleId,
            TenantId = tenantId,
            Name = "Manager",
            IsSystemRole = false,
            RolePermissions = new List<RolePermission>()
        };

        _roleRepositoryMock
            .Setup(r => r.GetByIdWithPermissionsAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRole);

        _roleRepositoryMock
            .Setup(r => r.ExistsByNameAsync("Admin", tenantId, roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // Name already taken

        var command = new UpdateRoleCommand
        {
            Id = roleId,
            Name = "Admin", // Duplicate name
            Description = "Description",
            Priority = 100,
            PermissionIds = new List<Guid> { Guid.NewGuid() }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new UpdateRoleCommand
        {
            Id = Guid.NewGuid(),
            Name = "New Name",
            Description = "Description",
            Priority = 100,
            PermissionIds = new List<Guid> { Guid.NewGuid() }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context not set");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdatePermissions()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var oldPermissionId = Guid.NewGuid();
        var newPermissionId1 = Guid.NewGuid();
        var newPermissionId2 = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var existingRole = new Role
        {
            Id = roleId,
            TenantId = tenantId,
            Name = "Manager",
            IsSystemRole = false,
            RolePermissions = new List<RolePermission>
            {
                new RolePermission { PermissionId = oldPermissionId }
            }
        };

        _roleRepositoryMock
            .Setup(r => r.GetByIdWithPermissionsAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRole);

        _roleRepositoryMock
            .Setup(r => r.ExistsByNameAsync("Manager", tenantId, roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var newPermissions = new List<Permission>
        {
            new Permission { Id = newPermissionId1, Name = "perm1", Resource = "res", Action = "read" },
            new Permission { Id = newPermissionId2, Name = "perm2", Resource = "res", Action = "write" }
        };

        _permissionRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.Is<List<Guid>>(ids => ids.Contains(newPermissionId1) && ids.Contains(newPermissionId2)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newPermissions);

        _roleRepositoryMock
            .Setup(r => r.GetUserCountAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var command = new UpdateRoleCommand
        {
            Id = roleId,
            Name = "Manager",
            Description = "Updated",
            Priority = 100,
            PermissionIds = new List<Guid> { newPermissionId1, newPermissionId2 }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingRole.RolePermissions.Should().HaveCount(2);
        existingRole.RolePermissions.Should().OnlyContain(rp =>
            rp.PermissionId == newPermissionId1 || rp.PermissionId == newPermissionId2);
    }
}
