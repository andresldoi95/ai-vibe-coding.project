using Xunit;
using FluentAssertions;
using SaaS.Application.Features.Users.Commands.RemoveUserFromCompany;

namespace Application.Tests.Features.Users.Commands;

public class RemoveUserFromCompanyCommandValidatorTests
{
    private readonly RemoveUserFromCompanyCommandValidator _validator;

    public RemoveUserFromCompanyCommandValidatorTests()
    {
        _validator = new RemoveUserFromCompanyCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new RemoveUserFromCompanyCommand(Guid.NewGuid());

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
        var command = new RemoveUserFromCompanyCommand(Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId" && e.ErrorMessage.Contains("required"));
    }
}
