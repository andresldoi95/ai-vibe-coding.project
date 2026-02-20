using Xunit;
using FluentAssertions;
using Moq;
using SaaS.Application.Features.Roles.Queries.GetRoleById;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.Roles.Queries;

public class GetRoleByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly GetRoleByIdQueryHandler _handler;

    public GetRoleByIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _roleRepositoryMock = new Mock<IRoleRepository>();

        _unitOfWorkMock.Setup(u => u.Roles).Returns(_roleRepositoryMock.Object);

        _handler = new GetRoleByIdQueryHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingRole_ShouldReturnRoleWithPermissions()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
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

        var role = new Role
        {
            Id = roleId,
            TenantId = tenantId,
            Name = "Manager",
            Description = "Warehouse Manager",
            Priority = 100,
            IsSystemRole = false,
            IsActive = true,
            RolePermissions = new List<RolePermission>
            {
                new RolePermission { RoleId = roleId, PermissionId = permission1.Id, Permission = permission1 },
                new RolePermission { RoleId = roleId, PermissionId = permission2.Id, Permission = permission2 }
            }
        };

        _roleRepositoryMock
            .Setup(r => r.GetByIdWithPermissionsAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _roleRepositoryMock
            .Setup(r => r.GetUserCountAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var query = new GetRoleByIdQuery { Id = roleId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(roleId);
        result.Value.Name.Should().Be("Manager");
        result.Value.Description.Should().Be("Warehouse Manager");
        result.Value.Priority.Should().Be(100);
        result.Value.IsSystemRole.Should().BeFalse();
        result.Value.IsActive.Should().BeTrue();
        result.Value.UserCount.Should().Be(5);
        result.Value.Permissions.Should().HaveCount(2);
        result.Value.Permissions.Should().Contain(p => p.Resource == "Warehouses" && p.Action == "Create");
        result.Value.Permissions.Should().Contain(p => p.Resource == "Products" && p.Action == "Read");
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

        var query = new GetRoleByIdQuery { Id = Guid.NewGuid() };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

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

        var query = new GetRoleByIdQuery { Id = roleId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Role not found");
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var query = new GetRoleByIdQuery { Id = Guid.NewGuid() };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context not set");
    }

    [Fact]
    public async Task Handle_RoleWithNoPermissions_ShouldReturnEmptyPermissionsList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var role = new Role
        {
            Id = roleId,
            TenantId = tenantId,
            Name = "Limited Role",
            Description = "Role with no permissions",
            Priority = 50,
            IsSystemRole = false,
            IsActive = true,
            RolePermissions = new List<RolePermission>() // No permissions
        };

        _roleRepositoryMock
            .Setup(r => r.GetByIdWithPermissionsAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _roleRepositoryMock
            .Setup(r => r.GetUserCountAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var query = new GetRoleByIdQuery { Id = roleId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(roleId);
        result.Value.Name.Should().Be("Limited Role");
        result.Value.Permissions.Should().BeEmpty();
        result.Value.UserCount.Should().Be(0);
    }
}
