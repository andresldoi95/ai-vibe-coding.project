using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.Auth.Commands.Register;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.Auth.Commands;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<RegisterCommandHandler>> _loggerMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITenantRepository> _tenantRepositoryMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly Mock<IUserTenantRepository> _userTenantRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<RegisterCommandHandler>>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _tenantRepositoryMock = new Mock<ITenantRepository>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _permissionRepositoryMock = new Mock<IPermissionRepository>();
        _userTenantRepositoryMock = new Mock<IUserTenantRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        // Setup UnitOfWork to return mocked repositories
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Tenants).Returns(_tenantRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Roles).Returns(_roleRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Permissions).Returns(_permissionRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.UserTenants).Returns(_userTenantRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.RefreshTokens).Returns(_refreshTokenRepositoryMock.Object);

        _handler = new RegisterCommandHandler(
            _unitOfWorkMock.Object,
            _authServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldRegisterUserAndCompany()
    {
        // Arrange
        var command = new RegisterCommand
        {
            CompanyName = "Test Company",
            Slug = "test-company",
            Email = "admin@test.com",
            Password = "SecurePassword123",
            Name = "Admin User"
        };

        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _tenantRepositoryMock
            .Setup(r => r.SlugExistsAsync(command.Slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _authServiceMock
            .Setup(a => a.HashPassword(command.Password))
            .Returns("hashed_password");

        _permissionRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Permission>
            {
                new Permission { Id = Guid.NewGuid(), Resource = "warehouses", Action = "read" },
                new Permission { Id = Guid.NewGuid(), Resource = "products", Action = "write" }
            });

        _authServiceMock
            .Setup(a => a.GenerateJwtToken(It.IsAny<User>(), It.IsAny<List<Guid>>()))
            .Returns("mock_jwt_token");

        _authServiceMock
            .Setup(a => a.GenerateRefreshToken(It.IsAny<string>()))
            .Returns(new RefreshToken
            {
                Token = "mock_refresh_token",
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AccessToken.Should().Be("mock_jwt_token");
        result.Value.RefreshToken.Should().Be("mock_refresh_token");
        result.Value.User.Should().NotBeNull();
        result.Value.User.Email.Should().Be(command.Email);
        result.Value.User.Name.Should().Be(command.Name);
        result.Value.Tenants.Should().HaveCount(1);
        result.Value.Tenants[0].Name.Should().Be(command.CompanyName);
        result.Value.Tenants[0].Slug.Should().Be(command.Slug);

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _tenantRepositoryMock.Verify(r => r.AddAsync(
            It.Is<Tenant>(t => t.Name == command.CompanyName && t.Slug == command.Slug),
            It.IsAny<CancellationToken>()),
            Times.Once);
        _userRepositoryMock.Verify(r => r.AddAsync(
            It.Is<User>(u => u.Email == command.Email && u.Name == command.Name),
            It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterCommand
        {
            CompanyName = "Test Company",
            Slug = "test-company",
            Email = "existing@test.com",
            Password = "SecurePassword123",
            Name = "Admin User"
        };

        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Email already registered");

        _tenantRepositoryMock.Verify(r => r.AddAsync(
            It.IsAny<Tenant>(),
            It.IsAny<CancellationToken>()),
            Times.Never);

        _userRepositoryMock.Verify(r => r.AddAsync(
            It.IsAny<User>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ExistingSlug_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterCommand
        {
            CompanyName = "Test Company",
            Slug = "existing-slug",
            Email = "admin@test.com",
            Password = "SecurePassword123",
            Name = "Admin User"
        };

        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _tenantRepositoryMock
            .Setup(r => r.SlugExistsAsync(command.Slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Company slug already taken");

        _tenantRepositoryMock.Verify(r => r.AddAsync(
            It.IsAny<Tenant>(),
            It.IsAny<CancellationToken>()),
            Times.Never);

        _userRepositoryMock.Verify(r => r.AddAsync(
            It.IsAny<User>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateDefaultRoles()
    {
        // Arrange
        var command = new RegisterCommand
        {
            CompanyName = "Test Company",
            Slug = "test-company",
            Email = "admin@test.com",
            Password = "SecurePassword123",
            Name = "Admin User"
        };

        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _tenantRepositoryMock
            .Setup(r => r.SlugExistsAsync(command.Slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _authServiceMock
            .Setup(a => a.HashPassword(command.Password))
            .Returns("hashed_password");

        _permissionRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Permission>
            {
                new Permission { Id = Guid.NewGuid(), Resource = "warehouses", Action = "read" },
                new Permission { Id = Guid.NewGuid(), Resource = "products", Action = "write" },
                new Permission { Id = Guid.NewGuid(), Resource = "tenants", Action = "manage" }
            });

        _authServiceMock
            .Setup(a => a.GenerateJwtToken(It.IsAny<User>(), It.IsAny<List<Guid>>()))
            .Returns("mock_jwt_token");

        _authServiceMock
            .Setup(a => a.GenerateRefreshToken(It.IsAny<string>()))
            .Returns(new RefreshToken { Token = "mock_refresh_token", ExpiresAt = DateTime.UtcNow.AddDays(7) });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify that roles were created (Owner, Admin, Manager, User = 4 roles)
        _roleRepositoryMock.Verify(r => r.AddAsync(
            It.IsAny<Role>(),
            It.IsAny<CancellationToken>()),
            Times.Exactly(4));
    }

    [Fact]
    public async Task Handle_ShouldCreateUserTenantRelationship()
    {
        // Arrange
        var command = new RegisterCommand
        {
            CompanyName = "Test Company",
            Slug = "test-company",
            Email = "admin@test.com",
            Password = "SecurePassword123",
            Name = "Admin User"
        };

        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _tenantRepositoryMock
            .Setup(r => r.SlugExistsAsync(command.Slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _authServiceMock
            .Setup(a => a.HashPassword(command.Password))
            .Returns("hashed_password");

        _permissionRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Permission>());

        _authServiceMock
            .Setup(a => a.GenerateJwtToken(It.IsAny<User>(), It.IsAny<List<Guid>>()))
            .Returns("mock_jwt_token");

        _authServiceMock
            .Setup(a => a.GenerateRefreshToken(It.IsAny<string>()))
            .Returns(new RefreshToken { Token = "mock_refresh_token", ExpiresAt = DateTime.UtcNow.AddDays(7) });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _userTenantRepositoryMock.Verify(r => r.AddAsync(
            It.Is<UserTenant>(ut => ut.IsActive == true),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Exception_ShouldRollbackTransaction()
    {
        // Arrange
        var command = new RegisterCommand
        {
            CompanyName = "Test Company",
            Slug = "test-company",
            Email = "admin@test.com",
            Password = "SecurePassword123",
            Name = "Admin User"
        };

        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _tenantRepositoryMock
            .Setup(r => r.SlugExistsAsync(command.Slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _tenantRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Registration failed");

        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldHashPassword()
    {
        // Arrange
        var command = new RegisterCommand
        {
            CompanyName = "Test Company",
            Slug = "test-company",
            Email = "admin@test.com",
            Password = "SecurePassword123",
            Name = "Admin User"
        };

        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _tenantRepositoryMock
            .Setup(r => r.SlugExistsAsync(command.Slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _authServiceMock
            .Setup(a => a.HashPassword(command.Password))
            .Returns("hashed_password");

        _permissionRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Permission>());

        _authServiceMock
            .Setup(a => a.GenerateJwtToken(It.IsAny<User>(), It.IsAny<List<Guid>>()))
            .Returns("mock_jwt_token");

        _authServiceMock
            .Setup(a => a.GenerateRefreshToken(It.IsAny<string>()))
            .Returns(new RefreshToken { Token = "mock_refresh_token", ExpiresAt = DateTime.UtcNow.AddDays(7) });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _authServiceMock.Verify(a => a.HashPassword(command.Password), Times.Once);
        _userRepositoryMock.Verify(r => r.AddAsync(
            It.Is<User>(u => u.PasswordHash == "hashed_password"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
