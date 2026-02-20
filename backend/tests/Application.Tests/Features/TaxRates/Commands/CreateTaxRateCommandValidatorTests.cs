using Xunit;
using FluentAssertions;
using SaaS.Application.Features.TaxRates.Commands.CreateTaxRate;

namespace Application.Tests.Features.TaxRates.Commands;

public class CreateTaxRateCommandValidatorTests
{
    private readonly CreateTaxRateCommandValidator _validator;

    public CreateTaxRateCommandValidatorTests()
    {
        _validator = new CreateTaxRateCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateTaxRateCommand
        {
            Code = "VAT-20",
            Name = "Value Added Tax 20%",
            Rate = 0.20m,
            Description = "Standard VAT rate"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidCommandWithoutDescription_ShouldPass()
    {
        // Arrange
        var command = new CreateTaxRateCommand
        {
            Code = "VAT-0",
            Name = "Zero Rate",
            Rate = 0m
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyCode_ShouldFail()
    {
        // Arrange
        var command = new CreateTaxRateCommand
        {
            Code = "",
            Name = "Tax Rate",
            Rate = 0.15m
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Code" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_CodeTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateTaxRateCommand
        {
            Code = new string('A', 51),
            Name = "Tax Rate",
            Rate = 0.15m
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Code" && e.ErrorMessage.Contains("50"));
    }

    [Fact]
    public void Validate_CodeExactly50Characters_ShouldPass()
    {
        // Arrange
        var command = new CreateTaxRateCommand
        {
            Code = new string('A', 50),
            Name = "Tax Rate",
            Rate = 0.15m
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyName_ShouldFail()
    {
        // Arrange
        var command = new CreateTaxRateCommand
        {
            Code = "TAX-01",
            Name = "",
            Rate = 0.15m
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
        var command = new CreateTaxRateCommand
        {
            Code = "TAX-01",
            Name = new string('A', 101),
            Rate = 0.15m
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("100"));
    }

    [Fact]
    public void Validate_NameExactly100Characters_ShouldPass()
    {
        // Arrange
        var command = new CreateTaxRateCommand
        {
            Code = "TAX-01",
            Name = new string('A', 100),
            Rate = 0.15m
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_RateNegative_ShouldFail()
    {
        // Arrange
        var command = new CreateTaxRateCommand
        {
            Code = "TAX-01",
            Name = "Tax Rate",
            Rate = -0.05m
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Rate" && e.ErrorMessage.Contains("greater than or equal to 0"));
    }

    [Fact]
    public void Validate_RateZero_ShouldPass()
    {
        // Arrange
        var command = new CreateTaxRateCommand
        {
            Code = "TAX-ZERO",
            Name = "Zero Rate",
            Rate = 0m
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_RateOne_ShouldPass()
    {
        // Arrange
        var command = new CreateTaxRateCommand
        {
            Code = "TAX-100",
            Name = "Full Rate",
            Rate = 1m
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_RateGreaterThanOne_ShouldFail()
    {
        // Arrange
        var command = new CreateTaxRateCommand
        {
            Code = "TAX-01",
            Name = "Tax Rate",
            Rate = 1.05m
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Rate" && e.ErrorMessage.Contains("less than or equal to 1"));
    }

    [Fact]
    public void Validate_DescriptionTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateTaxRateCommand
        {
            Code = "TAX-01",
            Name = "Tax Rate",
            Rate = 0.15m,
            Description = new string('A', 501)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description" && e.ErrorMessage.Contains("500"));
    }

    [Fact]
    public void Validate_DescriptionExactly500Characters_ShouldPass()
    {
        // Arrange
        var command = new CreateTaxRateCommand
        {
            Code = "TAX-01",
            Name = "Tax Rate",
            Rate = 0.15m,
            Description = new string('A', 500)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
