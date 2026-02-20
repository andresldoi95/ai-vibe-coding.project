using Xunit;
using FluentAssertions;
using SaaS.Application.Features.StockMovements.Commands.DeleteStockMovement;

namespace Application.Tests.Features.StockMovements.Commands;

public class DeleteStockMovementCommandValidatorTests
{
    private readonly DeleteStockMovementCommandValidator _validator;

    public DeleteStockMovementCommandValidatorTests()
    {
        _validator = new DeleteStockMovementCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new DeleteStockMovementCommand(Guid.NewGuid());

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
        var command = new DeleteStockMovementCommand(Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id" && e.ErrorMessage.Contains("required"));
    }
}
