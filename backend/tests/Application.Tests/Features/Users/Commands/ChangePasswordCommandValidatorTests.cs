using Xunit;
using FluentAssertions;
using SaaS.Application.Features.Users.Commands.ChangePassword;

namespace Application.Tests.Features.Users.Commands;

public class ChangePasswordCommandValidatorTests
{
    private readonly ChangePasswordCommandValidator _validator;

    public ChangePasswordCommandValidatorTests()
    {
        _validator = new ChangePasswordCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            CurrentPassword = "OldPassword123",
            NewPassword = "NewSecurePassword456",
            ConfirmPassword = "NewSecurePassword456"
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
    public void Validate_EmptyCurrentPassword_ShouldFail(string currentPassword)
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            CurrentPassword = currentPassword,
            NewPassword = "NewSecurePassword456",
            ConfirmPassword = "NewSecurePassword456"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CurrentPassword" && e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyNewPassword_ShouldFail(string newPassword)
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            CurrentPassword = "OldPassword123",
            NewPassword = newPassword,
            ConfirmPassword = newPassword
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword" && e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("short")]
    [InlineData("1234567")]
    public void Validate_NewPasswordTooShort_ShouldFail(string newPassword)
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            CurrentPassword = "OldPassword123",
            NewPassword = newPassword,
            ConfirmPassword = newPassword
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword" && e.ErrorMessage.Contains("at least 8 characters"));
    }

    [Fact]
    public void Validate_NewPasswordExactly8Characters_ShouldPass()
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            CurrentPassword = "OldPassword123",
            NewPassword = "12345678", // Exactly 8 characters
            ConfirmPassword = "12345678"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_NewPasswordTooLong_ShouldFail()
    {
        // Arrange
        var newPassword = new string('A', 101); // 101 characters
        var command = new ChangePasswordCommand
        {
            CurrentPassword = "OldPassword123",
            NewPassword = newPassword,
            ConfirmPassword = newPassword
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword" && e.ErrorMessage.Contains("100"));
    }

    [Fact]
    public void Validate_NewPasswordExactly100Characters_ShouldPass()
    {
        // Arrange
        var newPassword = new string('A', 100); // Exactly 100 characters
        var command = new ChangePasswordCommand
        {
            CurrentPassword = "OldPassword123",
            NewPassword = newPassword,
            ConfirmPassword = newPassword
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyConfirmPassword_ShouldFail(string confirmPassword)
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            CurrentPassword = "OldPassword123",
            NewPassword = "NewSecurePassword456",
            ConfirmPassword = confirmPassword
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ConfirmPassword" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_PasswordsDoNotMatch_ShouldFail()
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            CurrentPassword = "OldPassword123",
            NewPassword = "NewSecurePassword456",
            ConfirmPassword = "DifferentPassword789"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ConfirmPassword" && e.ErrorMessage.Contains("match"));
    }

    [Fact]
    public void Validate_AllFieldsEmpty_ShouldFailWithMultipleErrors()
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            CurrentPassword = string.Empty,
            NewPassword = string.Empty,
            ConfirmPassword = string.Empty
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(3);
        result.Errors.Should().Contain(e => e.PropertyName == "CurrentPassword");
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword");
        result.Errors.Should().Contain(e => e.PropertyName == "ConfirmPassword");
    }
}
