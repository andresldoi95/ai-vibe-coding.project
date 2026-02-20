using Xunit;
using FluentAssertions;
using Moq;
using SaaS.Application.Features.Users.Commands.AcceptInvitation;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Features.Users.Commands;

public class AcceptInvitationCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IUserInvitationRepository> _userInvitationRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUserTenantRepository> _userTenantRepositoryMock;
    private readonly Mock<ITenantRepository> _tenantRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<ILogger<AcceptInvitationCommandHandler>> _loggerMock;
    private readonly AcceptInvitationCommandHandler _handler;

    public AcceptInvitationCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _authServiceMock = new Mock<IAuthService>();
        _userInvitationRepositoryMock = new Mock<IUserInvitationRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _userTenantRepositoryMock = new Mock<IUserTenantRepository>();
        _tenantRepositoryMock = new Mock<ITenantRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _loggerMock = new Mock<ILogger<AcceptInvitationCommandHandler>>();

        _unitOfWorkMock.Setup(u => u.UserInvitations).Returns(_userInvitationRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.UserTenants).Returns(_userTenantRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Tenants).Returns(_tenantRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.RefreshTokens).Returns(_refreshTokenRepositoryMock.Object);

        _handler = new AcceptInvitationCommandHandler(
            _unitOfWorkMock.Object,
            _authServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_NewUserAcceptsInvitation_ShouldCreateUserTenantAndReturnToken()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var invitationToken = Guid.NewGuid().ToString();

        var invitation = new UserInvitation
        {
            Id = Guid.NewGuid(),
            InvitationToken = invitationToken,
            Email = "newuser@example.com",
            TenantId = tenantId,
            RoleId = roleId,
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            AcceptedAt = null
        };

        var user = new User
        {
            Id = userId,
            Email = "newuser@example.com",
            Name = string.Empty,
            PasswordHash = string.Empty,
            IsActive = false, // New user not yet active
            EmailConfirmed = false
        };

        var tenant = new Tenant { Id = tenantId, Name = "Test Company", Slug = "test-company" };

        _userInvitationRepositoryMock
            .Setup(r => r.GetByTokenAsync(invitationToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("newuser@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userTenantRepositoryMock
            .Setup(r => r.GetByUserAndTenantAsync(userId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserTenant?)null); // No existing UserTenant

        _tenantRepositoryMock
            .Setup(r => r.GetUserTenantsAsync(userId))
            .ReturnsAsync(new List<Tenant> { tenant });

        _authServiceMock
            .Setup(s => s.HashPassword("newPassword123"))
            .Returns("hashed-password");

        _authServiceMock
            .Setup(s => s.GenerateJwtToken(It.IsAny<User>(), It.IsAny<List<Guid>>()))
            .Returns("jwt-token");

        _authServiceMock
            .Setup(s => s.GenerateRefreshToken(It.IsAny<string>()))
            .Returns(new RefreshToken { Token = "refresh-token" });

        var command = new AcceptInvitationCommand
        {
            InvitationToken = invitationToken,
            Name = "New User",
            Password = "newPassword123"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccessToken.Should().Be("jwt-token");
        result.Value.RefreshToken.Should().Be("refresh-token");
        result.Value.User.Name.Should().Be("New User");
        result.Value.User.Email.Should().Be("newuser@example.com");
        
        user.Name.Should().Be("New User");
        user.IsActive.Should().BeTrue();
        user.EmailConfirmed.Should().BeTrue();
        invitation.IsActive.Should().BeFalse();
        invitation.AcceptedAt.Should().NotBeNull();
        
        _userRepositoryMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _userTenantRepositoryMock.Verify(r => r.AddAsync(It.IsAny<UserTenant>(), It.IsAny<CancellationToken>()), Times.Once);
        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidToken_ShouldReturnFailure()
    {
        // Arrange
        var invitationToken = "invalid-token";

        _userInvitationRepositoryMock
            .Setup(r => r.GetByTokenAsync(invitationToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserInvitation?)null);

        var command = new AcceptInvitationCommand
        {
            InvitationToken = invitationToken,
            Name = "User",
            Password = "password"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid invitation token");
    }

    [Fact]
    public async Task Handle_InactiveInvitation_ShouldReturnFailure()
    {
        // Arrange
        var invitationToken = Guid.NewGuid().ToString();
        var invitation = new UserInvitation
        {
            InvitationToken = invitationToken,
            IsActive = false, // Already used
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        _userInvitationRepositoryMock
            .Setup(r => r.GetByTokenAsync(invitationToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        var command = new AcceptInvitationCommand
        {
            InvitationToken = invitationToken,
            Name = "User",
            Password = "password"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("This invitation has already been used");
    }

    [Fact]
    public async Task Handle_ExpiredInvitation_ShouldReturnFailure()
    {
        // Arrange
        var invitationToken = Guid.NewGuid().ToString();
        var invitation = new UserInvitation
        {
            InvitationToken = invitationToken,
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
            AcceptedAt = null
        };

        _userInvitationRepositoryMock
            .Setup(r => r.GetByTokenAsync(invitationToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        var command = new AcceptInvitationCommand
        {
            InvitationToken = invitationToken,
            Name = "User",
            Password = "password"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("This invitation has expired");
    }

    [Fact]
    public async Task Handle_AlreadyAcceptedInvitation_ShouldReturnFailure()
    {
        // Arrange
        var invitationToken = Guid.NewGuid().ToString();
        var invitation = new UserInvitation
        {
            InvitationToken = invitationToken,
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            AcceptedAt = DateTime.UtcNow.AddHours(-1) // Already accepted
        };

        _userInvitationRepositoryMock
            .Setup(r => r.GetByTokenAsync(invitationToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        var command = new AcceptInvitationCommand
        {
            InvitationToken = invitationToken,
            Name = "User",
            Password = "password"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("This invitation has already been accepted");
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var invitationToken = Guid.NewGuid().ToString();
        var invitation = new UserInvitation
        {
            InvitationToken = invitationToken,
            Email = "user@example.com",
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            AcceptedAt = null
        };

        _userInvitationRepositoryMock
            .Setup(r => r.GetByTokenAsync(invitationToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new AcceptInvitationCommand
        {
            InvitationToken = invitationToken,
            Name = "User",
            Password = "password"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User not found");
    }

    [Fact]
    public async Task Handle_NewUserWithoutNamePassword_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var invitationToken = Guid.NewGuid().ToString();

        var invitation = new UserInvitation
        {
            InvitationToken = invitationToken,
            Email = "newuser@example.com",
            TenantId = tenantId,
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            AcceptedAt = null
        };

        var user = new User
        {
            Id = userId,
            Email = "newuser@example.com",
            IsActive = false, // New user
            EmailConfirmed = false
        };

        _userInvitationRepositoryMock
            .Setup(r => r.GetByTokenAsync(invitationToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("newuser@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new AcceptInvitationCommand
        {
            InvitationToken = invitationToken,
            Name = null, // Missing name
            Password = null // Missing password
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Name and password are required for new users");
    }

    [Fact]
    public async Task Handle_ExistingUserRejoinsCompany_ShouldReactivateUserTenant()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var invitationToken = Guid.NewGuid().ToString();

        var invitation = new UserInvitation
        {
            InvitationToken = invitationToken,
            Email = "existinguser@example.com",
            TenantId = tenantId,
            RoleId = roleId,
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            AcceptedAt = null
        };

        var user = new User
        {
            Id = userId,
            Email = "existinguser@example.com",
            Name = "Existing User",
            PasswordHash = "existing-hash",
            IsActive = true, // Already active user
            EmailConfirmed = true
        };

        var existingUserTenant = new UserTenant
        {
            UserId = userId,
            TenantId = tenantId,
            IsActive = false // Previously removed
        };

        var tenant = new Tenant { Id = tenantId, Name = "Test Company", Slug = "test-company" };

        _userInvitationRepositoryMock
            .Setup(r => r.GetByTokenAsync(invitationToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("existinguser@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userTenantRepositoryMock
            .Setup(r => r.GetByUserAndTenantAsync(userId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUserTenant);

        _tenantRepositoryMock
            .Setup(r => r.GetUserTenantsAsync(userId))
            .ReturnsAsync(new List<Tenant> { tenant });

        _authServiceMock
            .Setup(s => s.GenerateJwtToken(It.IsAny<User>(), It.IsAny<List<Guid>>()))
            .Returns("jwt-token");

        _authServiceMock
            .Setup(s => s.GenerateRefreshToken(It.IsAny<string>()))
            .Returns(new RefreshToken { Token = "refresh-token" });

        var command = new AcceptInvitationCommand
        {
            InvitationToken = invitationToken,
            Name = null, // Not needed for existing user
            Password = null // Not needed for existing user
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingUserTenant.IsActive.Should().BeTrue();
        existingUserTenant.RoleId.Should().Be(roleId);
        existingUserTenant.JoinedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        
        _userTenantRepositoryMock.Verify(r => r.UpdateAsync(existingUserTenant, It.IsAny<CancellationToken>()), Times.Once);
        _userTenantRepositoryMock.Verify(r => r.AddAsync(It.IsAny<UserTenant>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidAcceptance_ShouldGenerateTokensAndReturnLoginResponse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var invitationToken = Guid.NewGuid().ToString();

        var invitation = new UserInvitation
        {
            InvitationToken = invitationToken,
            Email = "user@example.com",
            TenantId = tenantId,
            RoleId = roleId,
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            AcceptedAt = null
        };

        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            Name = "Test User",
            PasswordHash = "hash",
            IsActive = true,
            EmailConfirmed = true
        };

        var tenant = new Tenant 
        { 
            Id = tenantId, 
            Name = "Test Company", 
            Slug = "test-company",
            Status = TenantStatus.Active
        };

        _userInvitationRepositoryMock
            .Setup(r => r.GetByTokenAsync(invitationToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userTenantRepositoryMock
            .Setup(r => r.GetByUserAndTenantAsync(userId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserTenant?)null);

        _tenantRepositoryMock
            .Setup(r => r.GetUserTenantsAsync(userId))
            .ReturnsAsync(new List<Tenant> { tenant });

        _authServiceMock
            .Setup(s => s.GenerateJwtToken(user, new List<Guid> { tenantId }))
            .Returns("generated-jwt-token");

        _authServiceMock
            .Setup(s => s.GenerateRefreshToken(It.IsAny<string>()))
            .Returns(new RefreshToken { Token = "generated-refresh-token" });

        var command = new AcceptInvitationCommand
        {
            InvitationToken = invitationToken,
            Name = null,
            Password = null
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("generated-jwt-token");
        result.Value.RefreshToken.Should().Be("generated-refresh-token");
        result.Value.User.Id.Should().Be(userId);
        result.Value.User.Email.Should().Be("user@example.com");
        result.Value.User.Name.Should().Be("Test User");
        result.Value.Tenants.Should().HaveCount(1);
        result.Value.Tenants.First().Id.Should().Be(tenantId);
        result.Value.Tenants.First().Name.Should().Be("Test Company");
        
        _authServiceMock.Verify(s => s.GenerateJwtToken(user, It.IsAny<List<Guid>>()), Times.Once);
        _authServiceMock.Verify(s => s.GenerateRefreshToken(It.IsAny<string>()), Times.Once);
    }
}
