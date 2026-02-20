using Xunit;
using FluentAssertions;
using Moq;
using SaaS.Application.Features.Auth.Commands.ForgotPassword;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.Auth.Commands;

public class ForgotPasswordCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly ForgotPasswordCommandHandler _handler;

    public ForgotPasswordCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _emailServiceMock = new Mock<IEmailService>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);

        _handler = new ForgotPasswordCommandHandler(
            _unitOfWorkMock.Object,
            _emailServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidEmail_ShouldGenerateResetTokenAndSendEmail()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            Name = "Test User",
            IsActive = true,
            PasswordHash = "hashed_password"
        };

        var command = new ForgotPasswordCommand
        {
            Email = "user@example.com"
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _userRepositoryMock.Verify(r => r.UpdateAsync(
            It.Is<User>(u => u.ResetToken != null && u.ResetTokenExpiry != null),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _emailServiceMock.Verify(e => e.SendPasswordResetAsync(
            user.Email,
            It.IsAny<string>()),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentEmail_ShouldReturnSuccessWithoutSendingEmail()
    {
        // Arrange
        var command = new ForgotPasswordCommand
        {
            Email = "nonexistent@example.com"
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Always return success to prevent email enumeration
        result.IsSuccess.Should().BeTrue();

        _emailServiceMock.Verify(e => e.SendPasswordResetAsync(
            It.IsAny<string>(),
            It.IsAny<string>()),
            Times.Never);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InactiveUser_ShouldReturnSuccessWithoutSendingEmail()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "inactive@example.com",
            Name = "Inactive User",
            IsActive = false,
            PasswordHash = "hashed_password"
        };

        var command = new ForgotPasswordCommand
        {
            Email = "inactive@example.com"
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Always return success to prevent user enumeration
        result.IsSuccess.Should().BeTrue();

        _emailServiceMock.Verify(e => e.SendPasswordResetAsync(
            It.IsAny<string>(),
            It.IsAny<string>()),
            Times.Never);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSetResetTokenExpiry()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            Name = "Test User",
            IsActive = true,
            PasswordHash = "hashed_password"
        };

        var command = new ForgotPasswordCommand
        {
            Email = "user@example.com"
        };

        var beforeTime = DateTime.UtcNow;

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _userRepositoryMock.Verify(r => r.UpdateAsync(
            It.Is<User>(u =>
                u.ResetTokenExpiry != null &&
                u.ResetTokenExpiry > beforeTime &&
                u.ResetTokenExpiry <= DateTime.UtcNow.AddHours(1).AddMinutes(1)), // 1 minute tolerance
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
