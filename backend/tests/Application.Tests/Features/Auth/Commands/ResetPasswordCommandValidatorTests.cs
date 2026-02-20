using Xunit;
using FluentAssertions;
using SaaS.Application.Features.Auth.Commands.ResetPassword;

namespace Application.Tests.Features.Auth.Commands;

public class ResetPasswordCommandValidatorTests
{
    private readonly ResetPasswordCommandValidator _validator;

    public ResetPasswordCommandValidatorTests()
    {
        _validator = new ResetPasswordCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            Token = "valid_reset_token_123",
            NewPassword = "NewSecurePass123!",
            ConfirmPassword = "NewSecurePass123!"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_EmptyToken_ShouldFail(string token)
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            Token = token,
            NewPassword = "NewSecurePass123!",
            ConfirmPassword = "NewSecurePass123!"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Token" && e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_EmptyNewPassword_ShouldFail(string password)
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            Token = "valid_reset_token_123",
            NewPassword = password,
            ConfirmPassword = password
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
    public void Validate_NewPasswordTooShort_ShouldFail(string password)
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            Token = "valid_reset_token_123",
            NewPassword = password,
            ConfirmPassword = password
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword" && e.ErrorMessage.Contains("at least 8 characters"));
    }

    [Theory]
    [InlineData("alllowercase123!")]
    [InlineData("nouppercase999!")]
    public void Validate_NewPasswordWithoutUppercase_ShouldFail(string password)
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            Token = "valid_reset_token_123",
            NewPassword = password,
            ConfirmPassword = password
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword" && e.ErrorMessage.Contains("uppercase letter"));
    }

    [Theory]
    [InlineData("ALLUPPERCASE123!")]
    [InlineData("NOLOWERCASE999!")]
    public void Validate_NewPasswordWithoutLowercase_ShouldFail(string password)
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            Token = "valid_reset_token_123",
            NewPassword = password,
            ConfirmPassword = password
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword" && e.ErrorMessage.Contains("lowercase letter"));
    }

    [Theory]
    [InlineData("NoNumbers!")]
    [InlineData("OnlyLetters!")]
    public void Validate_NewPasswordWithoutNumber_ShouldFail(string password)
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            Token = "valid_reset_token_123",
            NewPassword = password,
            ConfirmPassword = password
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword" && e.ErrorMessage.Contains("number"));
    }

    [Theory]
    [InlineData("NoSpecial123")]
    [InlineData("MissingSpecial999")]
    public void Validate_NewPasswordWithoutSpecialCharacter_ShouldFail(string password)
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            Token = "valid_reset_token_123",
            NewPassword = password,
            ConfirmPassword = password
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword" && e.ErrorMessage.Contains("special character"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_EmptyConfirmPassword_ShouldFail(string confirmPassword)
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            Token = "valid_reset_token_123",
            NewPassword = "NewSecurePass123!",
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
        var command = new ResetPasswordCommand
        {
            Token = "valid_reset_token_123",
            NewPassword = "NewSecurePass123!",
            ConfirmPassword = "DifferentPass456!"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ConfirmPassword" && e.ErrorMessage.Contains("do not match"));
    }
}
