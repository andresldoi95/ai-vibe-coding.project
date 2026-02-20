using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.Auth.Commands.RefreshToken;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.Auth.Commands;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<RefreshTokenCommandHandler>> _loggerMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<ITenantRepository> _tenantRepositoryMock;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<RefreshTokenCommandHandler>>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _tenantRepositoryMock = new Mock<ITenantRepository>();

        _unitOfWorkMock.Setup(u => u.RefreshTokens).Returns(_refreshTokenRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Tenants).Returns(_tenantRepositoryMock.Object);

        _handler = new RefreshTokenCommandHandler(
            _unitOfWorkMock.Object,
            _authServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_ShouldReturnNewAccessToken()
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
            PasswordHash = "hashed_password"
        };

        var validRefreshToken = new RefreshToken
        {
            Token = "valid_refresh_token",
            UserId = userId,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        var command = new RefreshTokenCommand
        {
            RefreshToken = "valid_refresh_token",
            IpAddress = "192.168.1.1"
        };

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validRefreshToken);

        _tenantRepositoryMock
            .Setup(r => r.GetUserTenantsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tenant>
            {
                new Tenant { Id = tenantId, Name = "Test Company", Slug = "test-company" }
            });

        _authServiceMock
            .Setup(a => a.GenerateRefreshToken(command.IpAddress))
            .Returns(new RefreshToken
            {
                Token = "new_refresh_token",
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });

        _authServiceMock
            .Setup(a => a.GenerateJwtToken(user, It.IsAny<List<Guid>>()))
            .Returns("new_access_token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("new_access_token");

        _refreshTokenRepositoryMock.Verify(r => r.RevokeTokenAsync(
            validRefreshToken,
            command.IpAddress,
            It.IsAny<CancellationToken>()),
            Times.Once);

        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(
            It.Is<RefreshToken>(rt => rt.Token == "new_refresh_token" && rt.UserId == userId),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidRefreshToken_ShouldReturnFailure()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            RefreshToken = "invalid_token",
            IpAddress = "192.168.1.1"
        };

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid refresh token");

        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(
            It.IsAny<RefreshToken>(),
            It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InactiveRefreshToken_ShouldReturnFailure()
    {
        // Arrange
        var expiredRefreshToken = new RefreshToken
        {
            Token = "expired_token",
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
            CreatedAt = DateTime.UtcNow.AddDays(-8)
        };

        var command = new RefreshTokenCommand
        {
            RefreshToken = "expired_token",
            IpAddress = "192.168.1.1"
        };

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredRefreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid refresh token");
    }

    [Fact]
    public async Task Handle_ShouldLogWarningOnInvalidToken()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            RefreshToken = "invalid_token",
            IpAddress = "192.168.1.1"
        };

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid refresh token attempt")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogInformationOnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            Name = "Test User",
            IsActive = true,
            PasswordHash = "hashed_password"
        };

        var validRefreshToken = new RefreshToken
        {
            Token = "valid_refresh_token",
            UserId = userId,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        var command = new RefreshTokenCommand
        {
            RefreshToken = "valid_refresh_token",
            IpAddress = "192.168.1.1"
        };

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validRefreshToken);

        _tenantRepositoryMock
            .Setup(r => r.GetUserTenantsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tenant>());

        _authServiceMock
            .Setup(a => a.GenerateRefreshToken(command.IpAddress))
            .Returns(new RefreshToken { Token = "new_refresh_token", ExpiresAt = DateTime.UtcNow.AddDays(7) });

        _authServiceMock
            .Setup(a => a.GenerateJwtToken(user, It.IsAny<List<Guid>>()))
            .Returns("new_access_token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Refresh token renewed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
