using Xunit;
using FluentAssertions;
using Moq;
using SaaS.Application.Features.Users.Commands.ChangePassword;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Features.Users.Commands;

public class ChangePasswordCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<ChangePasswordCommandHandler>> _loggerMock;
    private readonly ChangePasswordCommandHandler _handler;

    public ChangePasswordCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<ChangePasswordCommandHandler>>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);

        _handler = new ChangePasswordCommandHandler(
            _unitOfWorkMock.Object,
            _authServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidPasswordChange_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var oldPasswordHash = "old-hashed-password";
        var newPasswordHash = "new-hashed-password";

        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            Name = "Test User",
            PasswordHash = oldPasswordHash,
            IsActive = true,
            IsDeleted = false
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authServiceMock
            .Setup(s => s.VerifyPassword("currentPassword", oldPasswordHash))
            .Returns(true);

        _authServiceMock
            .Setup(s => s.HashPassword("newPassword"))
            .Returns(newPasswordHash);

        var command = new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = "currentPassword",
            NewPassword = "newPassword"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        
        user.PasswordHash.Should().Be(newPasswordHash);
        _userRepositoryMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyUserId_ShouldReturnFailure()
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            UserId = Guid.Empty,
            CurrentPassword = "currentPassword",
            NewPassword = "newPassword"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User not authenticated");
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = "currentPassword",
            NewPassword = "newPassword"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User not found");
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeletedUser_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            Name = "User Name",
            IsActive = false,
            IsDeleted = true
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = "currentPassword",
            NewPassword = "newPassword"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User not found");
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_IncorrectCurrentPassword_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            Name = "Test User",
            PasswordHash = "hashed-password",
            IsActive = true,
            IsDeleted = false
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authServiceMock
            .Setup(s => s.VerifyPassword("wrongPassword", "hashed-password"))
            .Returns(false);

        var command = new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = "wrongPassword",
            NewPassword = "newPassword"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Current password is incorrect");
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidPasswordChange_ShouldHashNewPassword()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            Name = "Test User",
            PasswordHash = "old-hash",
            IsActive = true,
            IsDeleted = false
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authServiceMock
            .Setup(s => s.VerifyPassword("oldPass", "old-hash"))
            .Returns(true);

        _authServiceMock
            .Setup(s => s.HashPassword("newPass123"))
            .Returns("hashed-newPass123");

        var command = new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = "oldPass",
            NewPassword = "newPass123"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Should().Be("hashed-newPass123");
        _authServiceMock.Verify(s => s.HashPassword("newPass123"), Times.Once);
    }
}
