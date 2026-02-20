using Xunit;
using FluentAssertions;
using Moq;
using SaaS.Application.Features.Roles.Commands.CreateRole;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.Roles.Commands;

public class CreateRoleCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly CreateRoleCommandHandler _handler;

    public CreateRoleCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _permissionRepositoryMock = new Mock<IPermissionRepository>();

        _unitOfWorkMock.Setup(u => u.Roles).Returns(_roleRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Permissions).Returns(_permissionRepositoryMock.Object);

        _handler = new CreateRoleCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateRole()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var permissionId1 = Guid.NewGuid();
        var permissionId2 = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _roleRepositoryMock
            .Setup(r => r.ExistsByNameAsync("Manager", tenantId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var permissions = new List<Permission>
        {
            new Permission { Id = permissionId1, Name = "invoices.create", Resource = "invoices", Action = "create" },
            new Permission { Id = permissionId2, Name = "invoices.read", Resource = "invoices", Action = "read" }
        };

        _permissionRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        _roleRepositoryMock
            .Setup(r => r.GetUserCountAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var command = new CreateRoleCommand
        {
            Name = "Manager",
            Description = "Manager role",
            Priority = 100,
            PermissionIds = new List<Guid> { permissionId1, permissionId2 }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Manager");
        result.Value.Description.Should().Be("Manager role");
        result.Value.Priority.Should().Be(100);
        result.Value.IsSystemRole.Should().BeFalse();
        result.Value.IsActive.Should().BeTrue();
        result.Value.Permissions.Should().HaveCount(2);

        _roleRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateRoleName_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _roleRepositoryMock
            .Setup(r => r.ExistsByNameAsync("Admin", tenantId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateRoleCommand
        {
            Name = "Admin",
            Description = "Admin role",
            Priority = 200,
            PermissionIds = new List<Guid> { Guid.NewGuid() }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");

        _roleRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new CreateRoleCommand
        {
            Name = "Manager",
            Description = "Manager role",
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
    public async Task Handle_EmptyTenantId_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns(Guid.Empty);

        var command = new CreateRoleCommand
        {
            Name = "Manager",
            Description = "Manager role",
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
    public async Task Handle_ValidCommand_ShouldCreateRolePermissions()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var permissionIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _roleRepositoryMock
            .Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), tenantId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var permissions = permissionIds.Select(id => new Permission
        {
            Id = id,
            Name = $"perm_{id}",
            Resource = "resource",
            Action = "action"
        }).ToList();

        _permissionRepositoryMock
            .Setup(r => r.GetByIdsAsync(permissionIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        _roleRepositoryMock
            .Setup(r => r.GetUserCountAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        Role? capturedRole = null;
        _roleRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()))
            .Callback<Role, CancellationToken>((role, _) => capturedRole = role)
            .ReturnsAsync((Role role, CancellationToken _) => role);

        var command = new CreateRoleCommand
        {
            Name = "Custom Role",
            Description = "Custom role description",
            Priority = 50,
            PermissionIds = permissionIds
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedRole.Should().NotBeNull();
        capturedRole!.RolePermissions.Should().HaveCount(3);
        capturedRole.RolePermissions.Should().OnlyContain(rp => permissionIds.Contains(rp.PermissionId));
    }
}
