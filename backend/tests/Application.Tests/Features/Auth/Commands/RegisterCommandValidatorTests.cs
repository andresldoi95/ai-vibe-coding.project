using Xunit;
using FluentAssertions;
using SaaS.Application.Features.Auth.Commands.Register;

namespace Application.Tests.Features.Auth.Commands;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator;

    public RegisterCommandValidatorTests()
    {
        _validator = new RegisterCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new RegisterCommand
        {
            CompanyName = "Test Company",
            Slug = "test-company",
            Email = "admin@test.com",
            Password = "SecurePass123",
            Name = "Admin User"
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
    public void Validate_EmptyCompanyName_ShouldFail(string companyName)
    {
        // Arrange
        var command = new RegisterCommand
        {
            CompanyName = companyName,
            Slug = "test-company",
            Email = "admin@test.com",
            Password = "SecurePass123",
            Name = "Admin User"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CompanyName" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_CompanyNameTooLong_ShouldFail()
    {
        // Arrange
        var command = new RegisterCommand
        {
            CompanyName = new string('A', 257), // 257 characters
            Slug = "test-company",
            Email = "admin@test.com",
            Password = "SecurePass123",
            Name = "Admin User"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CompanyName" && e.ErrorMessage.Contains("256"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_EmptySlug_ShouldFail(string slug)
    {
        // Arrange
        var command = new RegisterCommand
        {
            CompanyName = "Test Company",
            Slug = slug,
            Email = "admin@test.com",
            Password = "SecurePass123",
            Name = "Admin User"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Slug" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_SlugTooLong_ShouldFail()
    {
        // Arrange
        var command = new RegisterCommand
        {
            CompanyName = "Test Company",
            Slug = new string('a', 101), // 101 characters
            Email = "admin@test.com",
            Password = "SecurePass123",
            Name = "Admin User"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Slug" && e.ErrorMessage.Contains("100"));
    }

    [Theory]
    [InlineData("Invalid Slug")]
    [InlineData("UPPERCASE")]
    [InlineData("slug_underscore")]
    [InlineData("slug@special")]
    [InlineData("123números")]
    public void Validate_InvalidSlugFormat_ShouldFail(string slug)
    {
        // Arrange
        var command = new RegisterCommand
        {
            CompanyName = "Test Company",
            Slug = slug,
            Email = "admin@test.com",
            Password = "SecurePass123",
            Name = "Admin User"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Slug" && e.ErrorMessage.Contains("lowercase letters, numbers, and hyphens"));
    }

    [Theory]
    [InlineData("valid-slug")]
    [InlineData("slug123")]
    [InlineData("my-company-2024")]
    public void Validate_ValidSlugFormat_ShouldPass(string slug)
    {
        // Arrange
        var command = new RegisterCommand
        {
            CompanyName = "Test Company",
            Slug = slug,
            Email = "admin@test.com",
            Password = "SecurePass123",
            Name = "Admin User"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_EmptyEmail_ShouldFail(string email)
    {
        // Arrange
        var command = new RegisterCommand
        {
            CompanyName = "Test Company",
            Slug = "test-company",
            Email = email,
            Password = "SecurePass123",
            Name = "Admin User"
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
    public void Validate_InvalidEmailFormat_ShouldFail(string email)
    {
        // Arrange
        var command = new RegisterCommand
        {
            CompanyName = "Test Company",
            Slug = "test-company",
            Email = email,
            Password = "SecurePass123",
            Name = "Admin User"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("Invalid email"));
    }

    [Fact]
    public void Validate_EmailTooLong_ShouldFail()
    {
        // Arrange
        var command = new RegisterCommand
        {
            CompanyName = "Test Company",
            Slug = "test-company",
            Email = new string('a', 250) + "@test.com", // Over 256 characters
            Password = "SecurePass123",
            Name = "Admin User"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("256"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_EmptyPassword_ShouldFail(string password)
    {
        // Arrange
        var command = new RegisterCommand
        {
            CompanyName = "Test Company",
            Slug = "test-company",
            Email = "admin@test.com",
            Password = password,
            Name = "Admin User"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("short")]
    [InlineData("1234567")]
    public void Validate_PasswordTooShort_ShouldFail(string password)
    {
        // Arrange
        var command = new RegisterCommand
        {
            CompanyName = "Test Company",
            Slug = "test-company",
            Email = "admin@test.com",
            Password = password,
            Name = "Admin User"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("at least 8 characters"));
    }

    [Theory]
    [InlineData("alllowercase123")]
    [InlineData("nouppercase999")]
    public void Validate_PasswordWithoutUppercase_ShouldFail(string password)
    {
        // Arrange
        var command = new RegisterCommand
        {
            CompanyName = "Test Company",
            Slug = "test-company",
            Email = "admin@test.com",
            Password = password,
            Name = "Admin User"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("uppercase letter"));
    }

    [Theory]
    [InlineData("ALLUPPERCASE123")]
    [InlineData("NOLOWERCASE999")]
    public void Validate_PasswordWithoutLowercase_ShouldFail(string password)
    {
        // Arrange
        var command = new RegisterCommand
        {
            CompanyName = "Test Company",
            Slug = "test-company",
            Email = "admin@test.com",
            Password = password,
            Name = "Admin User"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("lowercase letter"));
    }

    [Theory]
    [InlineData("NoNumbers")]
    [InlineData("OnlyLetters")]
    public void Validate_PasswordWithoutNumber_ShouldFail(string password)
    {
        // Arrange
        var command = new RegisterCommand
        {
            CompanyName = "Test Company",
            Slug = "test-company",
            Email = "admin@test.com",
            Password = password,
            Name = "Admin User"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("number"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_EmptyName_ShouldFail(string name)
    {
        // Arrange
        var command = new RegisterCommand
        {
            CompanyName = "Test Company",
            Slug = "test-company",
            Email = "admin@test.com",
            Password = "SecurePass123",
            Name = name
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_NameTooLong_ShouldFail()
    {
        // Arrange
        var command = new RegisterCommand
        {
            CompanyName = "Test Company",
            Slug = "test-company",
            Email = "admin@test.com",
            Password = "SecurePass123",
            Name = new string('A', 257) // 257 characters
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("256"));
    }
}
