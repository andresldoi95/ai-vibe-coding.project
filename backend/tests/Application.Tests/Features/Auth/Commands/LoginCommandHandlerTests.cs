using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.Auth.Commands.Login;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace Application.Tests.Features.Auth.Commands;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<LoginCommandHandler>> _loggerMock;
    private readonly Mock<ITenantRepository> _tenantRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<LoginCommandHandler>>();
        _tenantRepositoryMock = new Mock<ITenantRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        // Setup UnitOfWork to return mocked repositories
        _unitOfWorkMock.Setup(u => u.Tenants).Returns(_tenantRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.RefreshTokens).Returns(_refreshTokenRepositoryMock.Object);

        _handler = new LoginCommandHandler(
            _unitOfWorkMock.Object,
            _authServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            Name = "Test User",
            IsActive = true,
            EmailConfirmed = true,
            PasswordHash = "hashed_password"
        };

        var tenant = new Tenant
        {
            Id = tenantId,
            Name = "Test Company",
            Slug = "test-company",
            Status = TenantStatus.Active
        };

        var command = new LoginCommand
        {
            Email = "user@example.com",
            Password = "Password123",
            IpAddress = "192.168.1.1"
        };

        _authServiceMock
            .Setup(a => a.ValidateCredentialsAsync(command.Email, command.Password))
            .ReturnsAsync(user);

        _tenantRepositoryMock
            .Setup(r => r.GetUserTenantsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tenant> { tenant });

        _authServiceMock
            .Setup(a => a.GenerateJwtToken(user, It.IsAny<List<Guid>>()))
            .Returns("mock_jwt_token");

        _authServiceMock
            .Setup(a => a.GenerateRefreshToken(command.IpAddress))
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
        result.Value.User.Email.Should().Be("user@example.com");
        result.Value.User.Name.Should().Be("Test User");
        result.Value.Tenants.Should().HaveCount(1);
        result.Value.Tenants[0].Name.Should().Be("Test Company");

        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(
            It.Is<RefreshToken>(rt => rt.UserId == userId),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidCredentials_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "wrong@example.com",
            Password = "WrongPassword",
            IpAddress = "192.168.1.1"
        };

        _authServiceMock
            .Setup(a => a.ValidateCredentialsAsync(command.Email, command.Password))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid email or password");

        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(
            It.IsAny<RefreshToken>(),
            It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserWithMultipleTenants_ShouldReturnAllTenants()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenant1Id = Guid.NewGuid();
        var tenant2Id = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            Name = "Test User",
            IsActive = true,
            EmailConfirmed = true,
            PasswordHash = "hashed_password"
        };

        var tenants = new List<Tenant>
        {
            new Tenant
            {
                Id = tenant1Id,
                Name = "Company 1",
                Slug = "company-1",
                Status = TenantStatus.Active
            },
            new Tenant
            {
                Id = tenant2Id,
                Name = "Company 2",
                Slug = "company-2",
                Status = TenantStatus.Active
            }
        };

        var command = new LoginCommand
        {
            Email = "user@example.com",
            Password = "Password123",
            IpAddress = "192.168.1.1"
        };

        _authServiceMock
            .Setup(a => a.ValidateCredentialsAsync(command.Email, command.Password))
            .ReturnsAsync(user);

        _tenantRepositoryMock
            .Setup(r => r.GetUserTenantsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenants);

        _authServiceMock
            .Setup(a => a.GenerateJwtToken(user, It.IsAny<List<Guid>>()))
            .Returns("mock_jwt_token");

        _authServiceMock
            .Setup(a => a.GenerateRefreshToken(command.IpAddress))
            .Returns(new RefreshToken { Token = "mock_refresh_token", ExpiresAt = DateTime.UtcNow.AddDays(7) });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Tenants.Should().HaveCount(2);
        result.Value.Tenants[0].Name.Should().Be("Company 1");
        result.Value.Tenants[1].Name.Should().Be("Company 2");
    }

    [Fact]
    public async Task Handle_SuccessfulLogin_ShouldLogInformation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            Name = "Test User",
            IsActive = true,
            EmailConfirmed = true,
            PasswordHash = "hashed_password"
        };

        var command = new LoginCommand
        {
            Email = "user@example.com",
            Password = "Password123",
            IpAddress = "192.168.1.1"
        };

        _authServiceMock
            .Setup(a => a.ValidateCredentialsAsync(command.Email, command.Password))
            .ReturnsAsync(user);

        _tenantRepositoryMock
            .Setup(r => r.GetUserTenantsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tenant>());

        _authServiceMock
            .Setup(a => a.GenerateJwtToken(user, It.IsAny<List<Guid>>()))
            .Returns("mock_jwt_token");

        _authServiceMock
            .Setup(a => a.GenerateRefreshToken(command.IpAddress))
            .Returns(new RefreshToken { Token = "mock_refresh_token", ExpiresAt = DateTime.UtcNow.AddDays(7) });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("logged in successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_FailedLogin_ShouldLogWarning()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "wrong@example.com",
            Password = "WrongPassword",
            IpAddress = "192.168.1.1"
        };

        _authServiceMock
            .Setup(a => a.ValidateCredentialsAsync(command.Email, command.Password))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed login attempt")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
