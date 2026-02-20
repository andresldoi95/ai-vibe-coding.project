using Xunit;
using FluentAssertions;
using SaaS.Application.Features.Warehouses.Commands.DeleteWarehouse;

namespace Application.Tests.Features.Warehouses.Commands;

public class DeleteWarehouseCommandValidatorTests
{
    private readonly DeleteWarehouseCommandValidator _validator;

    public DeleteWarehouseCommandValidatorTests()
    {
        _validator = new DeleteWarehouseCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new DeleteWarehouseCommand
        {
            Id = Guid.NewGuid()
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
        var command = new DeleteWarehouseCommand
        {
            Id = Guid.Empty
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id" && e.ErrorMessage.Contains("required"));
    }
}
