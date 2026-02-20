using Xunit;
using FluentAssertions;
using SaaS.Application.Features.Users.Commands.UpdateUserRole;

namespace Application.Tests.Features.Users.Commands;

public class UpdateUserRoleCommandValidatorTests
{
    private readonly UpdateUserRoleCommandValidator _validator;

    public UpdateUserRoleCommandValidatorTests()
    {
        _validator = new UpdateUserRoleCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new UpdateUserRoleCommand
        {
            UserId = Guid.NewGuid(),
            NewRoleId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyUserId_ShouldFail()
    {
        // Arrange
        var command = new UpdateUserRoleCommand
        {
            UserId = Guid.Empty,
            NewRoleId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_EmptyNewRoleId_ShouldFail()
    {
        // Arrange
        var command = new UpdateUserRoleCommand
        {
            UserId = Guid.NewGuid(),
            NewRoleId = Guid.Empty
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewRoleId" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_BothIdsEmpty_ShouldFailWithTwoErrors()
    {
        // Arrange
        var command = new UpdateUserRoleCommand
        {
            UserId = Guid.Empty,
            NewRoleId = Guid.Empty
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
        result.Errors.Should().Contain(e => e.PropertyName == "NewRoleId");
    }
}
