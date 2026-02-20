using Xunit;
using FluentAssertions;
using SaaS.Application.Features.Payments.Commands.CompletePayment;

namespace Application.Tests.Features.Payments.Commands;

public class CompletePaymentCommandValidatorTests
{
    private readonly CompletePaymentCommandValidator _validator;

    public CompletePaymentCommandValidatorTests()
    {
        _validator = new CompletePaymentCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CompletePaymentCommand
        {
            Id = Guid.NewGuid(),
            Notes = "Payment completed successfully"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidCommandWithoutNotes_ShouldPass()
    {
        // Arrange
        var command = new CompletePaymentCommand
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
        var command = new CompletePaymentCommand
        {
            Id = Guid.Empty,
            Notes = "Payment completed"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_NotesTooLong_ShouldFail()
    {
        // Arrange
        var command = new CompletePaymentCommand
        {
            Id = Guid.NewGuid(),
            Notes = new string('A', 1001)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Notes" && e.ErrorMessage.Contains("1000"));
    }

    [Fact]
    public void Validate_NotesExactly1000Characters_ShouldPass()
    {
        // Arrange
        var command = new CompletePaymentCommand
        {
            Id = Guid.NewGuid(),
            Notes = new string('A', 1000)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
