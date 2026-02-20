using Xunit;
using FluentAssertions;
using Moq;
using SaaS.Application.Features.Auth.Queries.GetCurrentUser;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;

namespace Application.Tests.Features.Auth.Queries;

public class GetCurrentUserQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly GetCurrentUserQueryHandler _handler;

    public GetCurrentUserQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);

        _handler = new GetCurrentUserQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidUserId_ShouldReturnUserDto()
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

        var query = new GetCurrentUserQuery
        {
            UserId = userId
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(userId);
        result.Value.Email.Should().Be("user@example.com");
        result.Value.Name.Should().Be("Test User");
        result.Value.IsActive.Should().BeTrue();
        result.Value.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetCurrentUserQuery
        {
            UserId = userId
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User not found");
    }

    [Fact]
    public async Task Handle_ShouldMapAllUserProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "admin@company.com",
            Name = "Admin User",
            IsActive = false,
            EmailConfirmed = false,
            PasswordHash = "hashed_password"
        };

        var query = new GetCurrentUserQuery
        {
            UserId = userId
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(userId);
        result.Value.Email.Should().Be("admin@company.com");
        result.Value.Name.Should().Be("Admin User");
        result.Value.IsActive.Should().BeFalse();
        result.Value.EmailConfirmed.Should().BeFalse();
    }
}
