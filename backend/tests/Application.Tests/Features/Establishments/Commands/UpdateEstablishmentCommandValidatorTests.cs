using Xunit;
using FluentAssertions;
using SaaS.Application.Features.Establishments.Commands.UpdateEstablishment;

namespace Application.Tests.Features.Establishments.Commands;

public class UpdateEstablishmentCommandValidatorTests
{
    private readonly UpdateEstablishmentCommandValidator _validator;

    public UpdateEstablishmentCommandValidatorTests()
    {
        _validator = new UpdateEstablishmentCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new UpdateEstablishmentCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentCode = "001",
            Name = "Main Office",
            Address = "123 Main St",
            Phone = "555-1234",
            Email = "office@example.com"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidCommandWithoutOptionalFields_ShouldPass()
    {
        // Arrange
        var command = new UpdateEstablishmentCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentCode = "001",
            Name = "Main Office"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyId_ShouldFail()
    {
        // Arrange
        var command = new UpdateEstablishmentCommand
        {
            Id = Guid.Empty,
            EstablishmentCode = "001",
            Name = "Main Office"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id" && e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyEstablishmentCode_ShouldFail(string code)
    {
        // Arrange
        var command = new UpdateEstablishmentCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentCode = code,
            Name = "Main Office"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EstablishmentCode" && e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("1")]
    [InlineData("12")]
    public void Validate_EstablishmentCodeTooShort_ShouldFail(string code)
    {
        // Arrange
        var command = new UpdateEstablishmentCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentCode = code,
            Name = "Main Office"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EstablishmentCode" && e.ErrorMessage.Contains("3 digits"));
    }

    [Theory]
    [InlineData("ABC")]
    [InlineData("A12")]
    public void Validate_EstablishmentCodeWithLetters_ShouldFail(string code)
    {
        // Arrange
        var command = new UpdateEstablishmentCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentCode = code,
            Name = "Main Office"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EstablishmentCode" && e.ErrorMessage.Contains("digits"));
    }

    [Fact]
    public void Validate_EstablishmentCodeZero_ShouldFail()
    {
        // Arrange
        var command = new UpdateEstablishmentCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentCode = "000",
            Name = "Main Office"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EstablishmentCode" && e.ErrorMessage.Contains("001 and 999"));
    }

    [Theory]
    [InlineData("001")]
    [InlineData("999")]
    public void Validate_EstablishmentCodeValidBoundaries_ShouldPass(string code)
    {
        // Arrange
        var command = new UpdateEstablishmentCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentCode = code,
            Name = "Main Office"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyName_ShouldFail(string name)
    {
        // Arrange
        var command = new UpdateEstablishmentCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentCode = "001",
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
        var command = new UpdateEstablishmentCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentCode = "001",
            Name = new string('A', 257)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("256"));
    }

    [Fact]
    public void Validate_AddressTooLong_ShouldFail()
    {
        // Arrange
        var command = new UpdateEstablishmentCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentCode = "001",
            Name = "Main Office",
            Address = new string('A', 501)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Address" && e.ErrorMessage.Contains("500"));
    }

    [Fact]
    public void Validate_PhoneTooLong_ShouldFail()
    {
        // Arrange
        var command = new UpdateEstablishmentCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentCode = "001",
            Name = "Main Office",
            Phone = new string('1', 51)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Phone" && e.ErrorMessage.Contains("50"));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    public void Validate_InvalidEmailFormat_ShouldFail(string email)
    {
        // Arrange
        var command = new UpdateEstablishmentCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentCode = "001",
            Name = "Main Office",
            Email = email
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("email"));
    }

    [Fact]
    public void Validate_EmailTooLong_ShouldFail()
    {
        // Arrange
        var command = new UpdateEstablishmentCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentCode = "001",
            Name = "Main Office",
            Email = new string('a', 250) + "@test.com" // > 256 characters
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("256"));
    }
}
