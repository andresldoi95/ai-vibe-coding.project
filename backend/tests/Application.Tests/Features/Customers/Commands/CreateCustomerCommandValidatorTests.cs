using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using SaaS.Application.Features.Customers.Commands.CreateCustomer;

namespace Application.Tests.Features.Customers.Commands;

public class CreateCustomerCommandValidatorTests
{
    private readonly CreateCustomerCommandValidator _validator;

    public CreateCustomerCommandValidatorTests()
    {
        _validator = new CreateCustomerCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            Name = "Test Customer",
            Email = "test@example.com",
            Phone = "+1234567890",
            TaxId = "1234567890",
            ContactPerson = "John Doe",
            BillingStreet = "123 Main St",
            BillingCity = "New York",
            BillingState = "NY",
            BillingPostalCode = "10001",
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyName_ShouldFail(string? name)
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            Name = name!,
            Email = "test@example.com",
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Customer name is required");
    }

    [Fact]
    public void Validate_NameTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            Name = new string('a', 257),
            Email = "test@example.com",
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Customer name cannot exceed 256 characters");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyEmail_ShouldFail(string? email)
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            Name = "Test Customer",
            Email = email!,
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test")]
    public void Validate_InvalidEmailFormat_ShouldFail(string email)
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            Name = "Test Customer",
            Email = email,
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Invalid email format");
    }

    [Fact]
    public void Validate_EmailTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            Name = "Test Customer",
            Email = new string('a', 250) + "@example.com",
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email cannot exceed 256 characters");
    }

    [Fact]
    public void Validate_PhoneTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            Name = "Test Customer",
            Email = "test@example.com",
            Phone = new string('1', 51),
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Phone)
            .WithErrorMessage("Phone cannot exceed 50 characters");
    }

    [Fact]
    public void Validate_TaxIdTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            Name = "Test Customer",
            Email = "test@example.com",
            TaxId = new string('1', 51),
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TaxId)
            .WithErrorMessage("Tax ID cannot exceed 50 characters");
    }

    [Fact]
    public void Validate_BillingStreetTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            Name = "Test Customer",
            Email = "test@example.com",
            BillingStreet = new string('a', 513),
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BillingStreet)
            .WithErrorMessage("Billing street cannot exceed 512 characters");
    }

    [Fact]
    public void Validate_BillingCityTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            Name = "Test Customer",
            Email = "test@example.com",
            BillingCity = new string('a', 101),
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BillingCity)
            .WithErrorMessage("Billing city cannot exceed 100 characters");
    }

    [Fact]
    public void Validate_NotesTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            Name = "Test Customer",
            Email = "test@example.com",
            Notes = new string('a', 2001),
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Notes)
            .WithErrorMessage("Notes cannot exceed 2000 characters");
    }

    [Fact]
    public void Validate_WebsiteTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            Name = "Test Customer",
            Email = "test@example.com",
            Website = new string('a', 257),
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Website)
            .WithErrorMessage("Website cannot exceed 256 characters");
    }

    [Fact]
    public void Validate_MinimalCustomer_ShouldPass()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            Name = "Minimal Customer",
            Email = "minimal@example.com",
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
