using Xunit;
using FluentAssertions;
using SaaS.Application.Features.Users.Commands.UpdateCurrentUser;

namespace Application.Tests.Features.Users.Commands;

public class UpdateCurrentUserCommandValidatorTests
{
    private readonly UpdateCurrentUserCommandValidator _validator;

    public UpdateCurrentUserCommandValidatorTests()
    {
        _validator = new UpdateCurrentUserCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new UpdateCurrentUserCommand
        {
            Name = "John Doe"
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
    public void Validate_EmptyName_ShouldFail(string name)
    {
        // Arrange
        var command = new UpdateCurrentUserCommand
        {
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
        var command = new UpdateCurrentUserCommand
        {
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
        var command = new UpdateCurrentUserCommand
        {
            Name = new string('A', 256) // Exactly 256 characters
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
