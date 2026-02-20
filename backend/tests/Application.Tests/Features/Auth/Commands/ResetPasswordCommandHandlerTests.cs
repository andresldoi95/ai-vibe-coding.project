using Xunit;
using FluentAssertions;
using Moq;
using SaaS.Application.Features.Auth.Commands.ResetPassword;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.Auth.Commands;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly ResetPasswordCommandHandler _handler;

    public ResetPasswordCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _authServiceMock = new Mock<IAuthService>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);

        _handler = new ResetPasswordCommandHandler(
            _unitOfWorkMock.Object,
            _authServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidToken_ShouldResetPasswordAndClearToken()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            Name = "Test User",
            IsActive = true,
            PasswordHash = "old_hashed_password",
            ResetToken = "valid_reset_token",
            ResetTokenExpiry = DateTime.UtcNow.AddHours(1)
        };

        var command = new ResetPasswordCommand
        {
            Token = "valid_reset_token",
            NewPassword = "NewSecurePassword123"
        };

        _userRepositoryMock
            .Setup(r => r.GetByResetTokenAsync(command.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authServiceMock
            .Setup(a => a.HashPassword(command.NewPassword))
            .Returns("new_hashed_password");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _userRepositoryMock.Verify(r => r.UpdateAsync(
            It.Is<User>(u =>
                u.PasswordHash == "new_hashed_password" &&
                u.ResetToken == null &&
                u.ResetTokenExpiry == null),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidToken_ShouldReturnFailure()
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            Token = "invalid_token",
            NewPassword = "NewSecurePassword123"
        };

        _userRepositoryMock
            .Setup(r => r.GetByResetTokenAsync(command.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid or expired reset token");

        _userRepositoryMock.Verify(r => r.UpdateAsync(
            It.IsAny<User>(),
            It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ExpiredToken_ShouldReturnFailure()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            Name = "Test User",
            IsActive = true,
            PasswordHash = "old_hashed_password",
            ResetToken = "expired_token",
            ResetTokenExpiry = DateTime.UtcNow.AddHours(-1) // Expired 1 hour ago
        };

        var command = new ResetPasswordCommand
        {
            Token = "expired_token",
            NewPassword = "NewSecurePassword123"
        };

        _userRepositoryMock
            .Setup(r => r.GetByResetTokenAsync(command.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Reset token has expired");

        _userRepositoryMock.Verify(r => r.UpdateAsync(
            It.IsAny<User>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_InactiveUser_ShouldReturnFailure()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            Name = "Test User",
            IsActive = false,
            PasswordHash = "old_hashed_password",
            ResetToken = "valid_token",
            ResetTokenExpiry = DateTime.UtcNow.AddHours(1)
        };

        var command = new ResetPasswordCommand
        {
            Token = "valid_token",
            NewPassword = "NewSecurePassword123"
        };

        _userRepositoryMock
            .Setup(r => r.GetByResetTokenAsync(command.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User account is inactive");

        _userRepositoryMock.Verify(r => r.UpdateAsync(
            It.IsAny<User>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldHashNewPassword()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            Name = "Test User",
            IsActive = true,
            PasswordHash = "old_hashed_password",
            ResetToken = "valid_token",
            ResetTokenExpiry = DateTime.UtcNow.AddHours(1)
        };

        var command = new ResetPasswordCommand
        {
            Token = "valid_token",
            NewPassword = "NewSecurePassword123"
        };

        _userRepositoryMock
            .Setup(r => r.GetByResetTokenAsync(command.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authServiceMock
            .Setup(a => a.HashPassword(command.NewPassword))
            .Returns("new_hashed_password");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _authServiceMock.Verify(a => a.HashPassword(command.NewPassword), Times.Once);
    }
}
