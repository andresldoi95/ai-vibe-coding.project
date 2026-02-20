using Xunit;
using FluentAssertions;
using SaaS.Application.Features.Users.Commands.InviteUser;

namespace Application.Tests.Features.Users.Commands;

public class InviteUserCommandValidatorTests
{
    private readonly InviteUserCommandValidator _validator;

    public InviteUserCommandValidatorTests()
    {
        _validator = new InviteUserCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new InviteUserCommand
        {
            Email = "newuser@example.com",
            RoleId = Guid.NewGuid(),
            PersonalMessage = "Welcome to our team!"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidCommandWithoutPersonalMessage_ShouldPass()
    {
        // Arrange
        var command = new InviteUserCommand
        {
            Email = "newuser@example.com",
            RoleId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyEmail_ShouldFail(string email)
    {
        // Arrange
        var command = new InviteUserCommand
        {
            Email = email,
            RoleId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user.example.com")]
    public void Validate_InvalidEmailFormat_ShouldFail(string email)
    {
        // Arrange
        var command = new InviteUserCommand
        {
            Email = email,
            RoleId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("Invalid email"));
    }

    [Fact]
    public void Validate_EmptyRoleId_ShouldFail()
    {
        // Arrange
        var command = new InviteUserCommand
        {
            Email = "newuser@example.com",
            RoleId = Guid.Empty
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RoleId" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_PersonalMessageTooLong_ShouldFail()
    {
        // Arrange
        var command = new InviteUserCommand
        {
            Email = "newuser@example.com",
            RoleId = Guid.NewGuid(),
            PersonalMessage = new string('A', 501) // 501 characters
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PersonalMessage" && e.ErrorMessage.Contains("500"));
    }

    [Fact]
    public void Validate_PersonalMessageExactly500Characters_ShouldPass()
    {
        // Arrange
        var command = new InviteUserCommand
        {
            Email = "newuser@example.com",
            RoleId = Guid.NewGuid(),
            PersonalMessage = new string('A', 500) // Exactly 500 characters
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
