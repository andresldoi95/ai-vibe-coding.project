using Xunit;
using FluentAssertions;
using SaaS.Application.Features.EmissionPoints.Commands.UpdateEmissionPoint;

namespace Application.Tests.Features.EmissionPoints.Commands;

public class UpdateEmissionPointCommandValidatorTests
{
    private readonly UpdateEmissionPointCommandValidator _validator;

    public UpdateEmissionPointCommandValidatorTests()
    {
        _validator = new UpdateEmissionPointCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new UpdateEmissionPointCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentId = Guid.NewGuid(),
            EmissionPointCode = "001",
            Name = "Main Store"
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
        var command = new UpdateEmissionPointCommand
        {
            Id = Guid.Empty,
            EstablishmentId = Guid.NewGuid(),
            EmissionPointCode = "001",
            Name = "Main Store"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_EmptyEstablishmentId_ShouldFail()
    {
        // Arrange
        var command = new UpdateEmissionPointCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentId = Guid.Empty,
            EmissionPointCode = "001",
            Name = "Main Store"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EstablishmentId" && e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyEmissionPointCode_ShouldFail(string code)
    {
        // Arrange
        var command = new UpdateEmissionPointCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentId = Guid.NewGuid(),
            EmissionPointCode = code,
            Name = "Main Store"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EmissionPointCode" && e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("1")]
    [InlineData("12")]
    public void Validate_EmissionPointCodeTooShort_ShouldFail(string code)
    {
        // Arrange
        var command = new UpdateEmissionPointCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentId = Guid.NewGuid(),
            EmissionPointCode = code,
            Name = "Main Store"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EmissionPointCode" && e.ErrorMessage.Contains("3 digits"));
    }

    [Theory]
    [InlineData("1234")]
    [InlineData("12345")]
    public void Validate_EmissionPointCodeTooLong_ShouldFail(string code)
    {
        // Arrange
        var command = new UpdateEmissionPointCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentId = Guid.NewGuid(),
            EmissionPointCode = code,
            Name = "Main Store"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EmissionPointCode" && e.ErrorMessage.Contains("3 digits"));
    }

    [Theory]
    [InlineData("ABC")]
    [InlineData("A12")]
    [InlineData("12A")]
    public void Validate_EmissionPointCodeWithLetters_ShouldFail(string code)
    {
        // Arrange
        var command = new UpdateEmissionPointCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentId = Guid.NewGuid(),
            EmissionPointCode = code,
            Name = "Main Store"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EmissionPointCode" && e.ErrorMessage.Contains("digits"));
    }

    [Fact]
    public void Validate_EmissionPointCodeZero_ShouldFail()
    {
        // Arrange
        var command = new UpdateEmissionPointCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentId = Guid.NewGuid(),
            EmissionPointCode = "000",
            Name = "Main Store"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EmissionPointCode" && e.ErrorMessage.Contains("001 and 999"));
    }

    [Theory]
    [InlineData("001")]
    [InlineData("999")]
    public void Validate_EmissionPointCodeValidBoundaries_ShouldPass(string code)
    {
        // Arrange
        var command = new UpdateEmissionPointCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentId = Guid.NewGuid(),
            EmissionPointCode = code,
            Name = "Main Store"
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
        var command = new UpdateEmissionPointCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentId = Guid.NewGuid(),
            EmissionPointCode = "001",
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
        var command = new UpdateEmissionPointCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentId = Guid.NewGuid(),
            EmissionPointCode = "001",
            Name = new string('A', 257) // 257 characters
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("256"));
    }

    [Fact]
    public void Validate_NameExactly256Characters_ShouldPass()
    {
        // Arrange
        var command = new UpdateEmissionPointCommand
        {
            Id = Guid.NewGuid(),
            EstablishmentId = Guid.NewGuid(),
            EmissionPointCode = "001",
            Name = new string('A', 256) // Exactly 256 characters
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
