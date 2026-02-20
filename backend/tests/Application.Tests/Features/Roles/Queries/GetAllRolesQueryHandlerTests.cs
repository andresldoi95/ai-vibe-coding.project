using Xunit;
using FluentAssertions;
using Moq;
using SaaS.Application.Features.Roles.Queries.GetAllRoles;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.Roles.Queries;

public class GetAllRolesQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly GetAllRolesQueryHandler _handler;

    public GetAllRolesQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _roleRepositoryMock = new Mock<IRoleRepository>();

        _unitOfWorkMock.Setup(u => u.Roles).Returns(_roleRepositoryMock.Object);

        _handler = new GetAllRolesQueryHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnAllRolesWithPermissions()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var permission1 = new Permission
        {
            Id = Guid.NewGuid(),
            Resource = "Warehouses",
            Action = "Create",
            Description = "Create warehouses"
        };

        var permission2 = new Permission
        {
            Id = Guid.NewGuid(),
            Resource = "Products",
            Action = "Read",
            Description = "View products"
        };

        var role1Id = Guid.NewGuid();
        var role2Id = Guid.NewGuid();

        var role1 = new Role
        {
            Id = role1Id,
            TenantId = tenantId,
            Name = "Admin",
            Description = "System Administrator",
            Priority = 1,
            IsSystemRole = true,
            IsActive = true,
            RolePermissions = new List<RolePermission>
            {
                new RolePermission { RoleId = role1Id, PermissionId = permission1.Id, Permission = permission1 },
                new RolePermission { RoleId = role1Id, PermissionId = permission2.Id, Permission = permission2 }
            }
        };

        var role2 = new Role
        {
            Id = role2Id,
            TenantId = tenantId,
            Name = "Manager",
            Description = "Warehouse Manager",
            Priority = 100,
            IsSystemRole = false,
            IsActive = true,
            RolePermissions = new List<RolePermission>
            {
                new RolePermission { RoleId = role2Id, PermissionId = permission1.Id, Permission = permission1 }
            }
        };

        var roles = new List<Role> { role1, role2 };

        _roleRepositoryMock
            .Setup(r => r.GetAllWithPermissionsByTenantAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        _roleRepositoryMock
            .Setup(r => r.GetUserCountAsync(role1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        _roleRepositoryMock
            .Setup(r => r.GetUserCountAsync(role2Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var query = new GetAllRolesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        var adminRole = result.Value.First(r => r.Name == "Admin");
        adminRole.Id.Should().Be(role1Id);
        adminRole.Priority.Should().Be(1);
        adminRole.IsSystemRole.Should().BeTrue();
        adminRole.UserCount.Should().Be(3);
        adminRole.Permissions.Should().HaveCount(2);

        var managerRole = result.Value.First(r => r.Name == "Manager");
        managerRole.Id.Should().Be(role2Id);
        managerRole.Priority.Should().Be(100);
        managerRole.IsSystemRole.Should().BeFalse();
        managerRole.UserCount.Should().Be(5);
        managerRole.Permissions.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_NoRoles_ShouldReturnEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _roleRepositoryMock
            .Setup(r => r.GetAllWithPermissionsByTenantAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role>());

        var query = new GetAllRolesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var query = new GetAllRolesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context not set");
    }

    [Fact]
    public async Task Handle_RolesWithNoPermissions_ShouldReturnRolesWithEmptyPermissions()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var roleId = Guid.NewGuid();
        var role = new Role
        {
            Id = roleId,
            TenantId = tenantId,
            Name = "Limited Role",
            Description = "No permissions",
            Priority = 200,
            IsSystemRole = false,
            IsActive = true,
            RolePermissions = new List<RolePermission>() // No permissions
        };

        _roleRepositoryMock
            .Setup(r => r.GetAllWithPermissionsByTenantAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role> { role });

        _roleRepositoryMock
            .Setup(r => r.GetUserCountAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var query = new GetAllRolesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Permissions.Should().BeEmpty();
        result.Value.First().UserCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_MultipleRoles_ShouldGetUserCountForEach()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var role1Id = Guid.NewGuid();
        var role2Id = Guid.NewGuid();
        var role3Id = Guid.NewGuid();

        var roles = new List<Role>
        {
            new Role { Id = role1Id, TenantId = tenantId, Name = "Role1", RolePermissions = new List<RolePermission>() },
            new Role { Id = role2Id, TenantId = tenantId, Name = "Role2", RolePermissions = new List<RolePermission>() },
            new Role { Id = role3Id, TenantId = tenantId, Name = "Role3", RolePermissions = new List<RolePermission>() }
        };

        _roleRepositoryMock
            .Setup(r => r.GetAllWithPermissionsByTenantAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        _roleRepositoryMock
            .Setup(r => r.GetUserCountAsync(role1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        _roleRepositoryMock
            .Setup(r => r.GetUserCountAsync(role2Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        _roleRepositoryMock
            .Setup(r => r.GetUserCountAsync(role3Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var query = new GetAllRolesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value.First(r => r.Name == "Role1").UserCount.Should().Be(10);
        result.Value.First(r => r.Name == "Role2").UserCount.Should().Be(5);
        result.Value.First(r => r.Name == "Role3").UserCount.Should().Be(0);

        _roleRepositoryMock.Verify(r => r.GetUserCountAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }
}
