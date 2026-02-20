using Xunit;
using FluentAssertions;
using SaaS.Application.Features.Establishments.Commands.DeleteEstablishment;

namespace Application.Tests.Features.Establishments.Commands;

public class DeleteEstablishmentCommandValidatorTests
{
    private readonly DeleteEstablishmentCommandValidator _validator;

    public DeleteEstablishmentCommandValidatorTests()
    {
        _validator = new DeleteEstablishmentCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new DeleteEstablishmentCommand(Guid.NewGuid());

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
        var command = new DeleteEstablishmentCommand(Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id" && e.ErrorMessage.Contains("required"));
    }
}
