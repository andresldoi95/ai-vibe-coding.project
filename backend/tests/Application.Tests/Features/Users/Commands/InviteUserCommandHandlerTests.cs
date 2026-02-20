using Xunit;
using FluentAssertions;
using Moq;
using SaaS.Application.Features.Users.Commands.InviteUser;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Domain.Entities;
using Microsoft.Extensions.Logging;
using MediatR;

namespace Application.Tests.Features.Users.Commands;

public class InviteUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ITenantRepository> _tenantRepositoryMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUserTenantRepository> _userTenantRepositoryMock;
    private readonly Mock<IUserInvitationRepository> _userInvitationRepositoryMock;
    private readonly Mock<ILogger<InviteUserCommandHandler>> _loggerMock;
    private readonly InviteUserCommandHandler _handler;

    public InviteUserCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _emailServiceMock = new Mock<IEmailService>();
        _tenantRepositoryMock = new Mock<ITenantRepository>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _userTenantRepositoryMock = new Mock<IUserTenantRepository>();
        _userInvitationRepositoryMock = new Mock<IUserInvitationRepository>();
        _loggerMock = new Mock<ILogger<InviteUserCommandHandler>>();

        _unitOfWorkMock.Setup(u => u.Tenants).Returns(_tenantRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Roles).Returns(_roleRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.UserTenants).Returns(_userTenantRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.UserInvitations).Returns(_userInvitationRepositoryMock.Object);

        _handler = new InviteUserCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidInvitation_ShouldCreateInvitationAndSendEmail()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _tenantContextMock.Setup(t => t.UserId).Returns(userId);

        var tenant = new Tenant { Id = tenantId, Name = "Test Company" };
        var role = new Role { Id = roleId, TenantId = tenantId, Name = "Manager" };

        _tenantRepositoryMock
            .Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        _roleRepositoryMock
            .Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null); // User doesn't exist yet

        _userInvitationRepositoryMock
            .Setup(r => r.GetActiveByEmailAndTenantAsync(It.IsAny<string>(), tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserInvitation?)null); // No existing invitation

        _emailServiceMock
            .Setup(s => s.SendUserInvitationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), tenantId))
            .ReturnsAsync(Result.Success());

        var command = new InviteUserCommand
        {
            Email = "newuser@example.com",
            RoleId = roleId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        _userRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == "newuser@example.com"), It.IsAny<CancellationToken>()), Times.Once);
        _userInvitationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<UserInvitation>(), It.IsAny<CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(s => s.SendUserInvitationAsync("newuser@example.com", "Test Company", It.IsAny<string>(), tenantId), Times.Once);
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new InviteUserCommand
        {
            Email = "user@example.com",
            RoleId = Guid.NewGuid()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");
    }

    [Fact]
    public async Task Handle_NoUserContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns(Guid.NewGuid());
        _tenantContextMock.Setup(t => t.UserId).Returns((Guid?)null);

        var command = new InviteUserCommand
        {
            Email = "user@example.com",
            RoleId = Guid.NewGuid()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User context is required");
    }

    [Fact]
    public async Task Handle_TenantNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _tenantContextMock.Setup(t => t.UserId).Returns(userId);

        _tenantRepositoryMock
            .Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        var command = new InviteUserCommand
        {
            Email = "user@example.com",
            RoleId = Guid.NewGuid()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant not found");
    }

    [Fact]
    public async Task Handle_InvalidRole_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _tenantContextMock.Setup(t => t.UserId).Returns(userId);

        var tenant = new Tenant { Id = tenantId, Name = "Test Company" };

        _tenantRepositoryMock
            .Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        _roleRepositoryMock
            .Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        var command = new InviteUserCommand
        {
            Email = "user@example.com",
            RoleId = roleId
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
        var roleId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _tenantContextMock.Setup(t => t.UserId).Returns(userId);

        var tenant = new Tenant { Id = tenantId, Name = "Test Company" };
        var role = new Role { Id = roleId, TenantId = otherTenantId, Name = "Manager" }; // Different tenant

        _tenantRepositoryMock
            .Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        _roleRepositoryMock
            .Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        var command = new InviteUserCommand
        {
            Email = "user@example.com",
            RoleId = roleId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid role selected");
    }

    [Fact]
    public async Task Handle_UserAlreadyInCompany_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existingUserId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _tenantContextMock.Setup(t => t.UserId).Returns(userId);

        var tenant = new Tenant { Id = tenantId, Name = "Test Company" };
        var role = new Role { Id = roleId, TenantId = tenantId, Name = "Manager" };
        var existingUser = new User { Id = existingUserId, Email = "existing@example.com" };
        var userTenant = new UserTenant { UserId = existingUserId, TenantId = tenantId, IsActive = true };

        _tenantRepositoryMock
            .Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        _roleRepositoryMock
            .Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("existing@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _userTenantRepositoryMock
            .Setup(r => r.GetByUserAndTenantAsync(existingUserId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userTenant);

        var command = new InviteUserCommand
        {
            Email = "existing@example.com",
            RoleId = roleId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User already exists in this company");
    }

    [Fact]
    public async Task Handle_ActiveInvitationExists_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _tenantContextMock.Setup(t => t.UserId).Returns(userId);

        var tenant = new Tenant { Id = tenantId, Name = "Test Company" };
        var role = new Role { Id = roleId, TenantId = tenantId, Name = "Manager" };
        var existingInvitation = new UserInvitation { Email = "user@example.com", IsActive = true };

        _tenantRepositoryMock
            .Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        _roleRepositoryMock
            .Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userInvitationRepositoryMock
            .Setup(r => r.GetActiveByEmailAndTenantAsync("user@example.com", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingInvitation);

        var command = new InviteUserCommand
        {
            Email = "user@example.com",
            RoleId = roleId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("An active invitation already exists for this email");
    }

    [Fact]
    public async Task Handle_ExistingUserNotInCompany_ShouldReuseUser()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existingUserId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _tenantContextMock.Setup(t => t.UserId).Returns(userId);

        var tenant = new Tenant { Id = tenantId, Name = "Test Company" };
        var role = new Role { Id = roleId, TenantId = tenantId, Name = "Manager" };
        var existingUser = new User { Id = existingUserId, Email = "existing@example.com" };

        _tenantRepositoryMock
            .Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        _roleRepositoryMock
            .Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("existing@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _userTenantRepositoryMock
            .Setup(r => r.GetByUserAndTenantAsync(existingUserId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserTenant?)null); // Not in this company

        _userInvitationRepositoryMock
            .Setup(r => r.GetActiveByEmailAndTenantAsync("existing@example.com", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserInvitation?)null);

        _emailServiceMock
            .Setup(s => s.SendUserInvitationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), tenantId))
            .ReturnsAsync(Result.Success());

        var command = new InviteUserCommand
        {
            Email = "existing@example.com",
            RoleId = roleId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // User should not be created again
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        
        // Invitation should be created
        _userInvitationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<UserInvitation>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmailFailure_ShouldStillSucceed()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _tenantContextMock.Setup(t => t.UserId).Returns(userId);

        var tenant = new Tenant { Id = tenantId, Name = "Test Company" };
        var role = new Role { Id = roleId, TenantId = tenantId, Name = "Manager" };

        _tenantRepositoryMock
            .Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        _roleRepositoryMock
            .Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userInvitationRepositoryMock
            .Setup(r => r.GetActiveByEmailAndTenantAsync(It.IsAny<string>(), tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserInvitation?)null);

        _emailServiceMock
            .Setup(s => s.SendUserInvitationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), tenantId))
            .ReturnsAsync(Result.Failure("Email service unavailable"));

        var command = new InviteUserCommand
        {
            Email = "user@example.com",
            RoleId = roleId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(); // Should still succeed even if email fails
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
