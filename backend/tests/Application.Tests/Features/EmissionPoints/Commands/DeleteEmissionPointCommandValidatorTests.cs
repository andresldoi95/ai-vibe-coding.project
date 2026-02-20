using Xunit;
using FluentAssertions;
using SaaS.Application.Features.EmissionPoints.Commands.DeleteEmissionPoint;

namespace Application.Tests.Features.EmissionPoints.Commands;

public class DeleteEmissionPointCommandValidatorTests
{
    private readonly DeleteEmissionPointCommandValidator _validator;

    public DeleteEmissionPointCommandValidatorTests()
    {
        _validator = new DeleteEmissionPointCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new DeleteEmissionPointCommand(Guid.NewGuid());

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
        var command = new DeleteEmissionPointCommand(Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id" && e.ErrorMessage.Contains("required"));
    }
}
