using Xunit;
using FluentAssertions;
using SaaS.Application.Features.Users.Commands.AcceptInvitation;

namespace Application.Tests.Features.Users.Commands;

public class AcceptInvitationCommandValidatorTests
{
    private readonly AcceptInvitationCommandValidator _validator;

    public AcceptInvitationCommandValidatorTests()
    {
        _validator = new AcceptInvitationCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new AcceptInvitationCommand
        {
            InvitationToken = "valid_token_12345",
            Name = "John Doe",
            Password = "SecurePassword123!"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidCommandWithoutPassword_ShouldPass()
    {
        // Arrange - Existing user accepting invitation
        var command = new AcceptInvitationCommand
        {
            InvitationToken = "valid_token_12345",
            Name = string.Empty,
            Password = string.Empty
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
    public void Validate_EmptyInvitationToken_ShouldFail(string token)
    {
        // Arrange
        var command = new AcceptInvitationCommand
        {
            InvitationToken = token,
            Name = "John Doe",
            Password = "SecurePassword123!"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "InvitationToken" && e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("short")]
    [InlineData("1234567")]
    public void Validate_PasswordTooShort_WhenProvided_ShouldFail(string password)
    {
        // Arrange
        var command = new AcceptInvitationCommand
        {
            InvitationToken = "valid_token_12345",
            Name = "John Doe",
            Password = password
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("at least 8 characters"));
    }

    [Fact]
    public void Validate_PasswordExactly8Characters_ShouldPass()
    {
        // Arrange
        var command = new AcceptInvitationCommand
        {
            InvitationToken = "valid_token_12345",
            Name = "John Doe",
            Password = "12345678" // Exactly 8 characters
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
