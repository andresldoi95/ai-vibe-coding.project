using Xunit;
using FluentAssertions;
using SaaS.Application.Features.Payments.Commands.VoidPayment;

namespace Application.Tests.Features.Payments.Commands;

public class VoidPaymentCommandValidatorTests
{
    private readonly VoidPaymentCommandValidator _validator;

    public VoidPaymentCommandValidatorTests()
    {
        _validator = new VoidPaymentCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new VoidPaymentCommand
        {
            Id = Guid.NewGuid(),
            Reason = "Duplicate payment"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidCommandWithoutReason_ShouldPass()
    {
        // Arrange
        var command = new VoidPaymentCommand
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
        var command = new VoidPaymentCommand
        {
            Id = Guid.Empty,
            Reason = "Voiding payment"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_ReasonTooLong_ShouldFail()
    {
        // Arrange
        var command = new VoidPaymentCommand
        {
            Id = Guid.NewGuid(),
            Reason = new string('A', 501)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Reason" && e.ErrorMessage.Contains("500"));
    }

    [Fact]
    public void Validate_ReasonExactly500Characters_ShouldPass()
    {
        // Arrange
        var command = new VoidPaymentCommand
        {
            Id = Guid.NewGuid(),
            Reason = new string('A', 500)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
