using Xunit;
using FluentAssertions;
using SaaS.Application.Features.Warehouses.Commands.CreateWarehouse;

namespace Application.Tests.Features.Warehouses.Commands;

public class CreateWarehouseCommandValidatorTests
{
    private readonly CreateWarehouseCommandValidator _validator;

    public CreateWarehouseCommandValidatorTests()
    {
        _validator = new CreateWarehouseCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateWarehouseCommand
        {
            Name = "Main Warehouse",
            Code = "WH-001",
            Description = "Primary storage facility",
            StreetAddress = "123 Storage St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            CountryId = Guid.NewGuid(),
            Phone = "+1-555-0100",
            Email = "warehouse@example.com",
            IsActive = true,
            SquareFootage = 50000m,
            Capacity = 10000
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_EmptyName_ShouldFail(string? name)
    {
        // Arrange
        var command = new CreateWarehouseCommand
        {
            Name = name!,
            Code = "WH-001",
            StreetAddress = "123 Storage St",
            City = "New York",
            PostalCode = "10001",
            CountryId = Guid.NewGuid()
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
        var command = new CreateWarehouseCommand
        {
            Name = new string('A', 257), // 257 characters
            Code = "WH-001",
            StreetAddress = "123 Storage St",
            City = "New York",
            PostalCode = "10001",
            CountryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("256"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_EmptyCode_ShouldFail(string? code)
    {
        // Arrange
        var command = new CreateWarehouseCommand
        {
            Name = "Main Warehouse",
            Code = code!,
            StreetAddress = "123 Storage St",
            City = "New York",
            PostalCode = "10001",
            CountryId = Guid.NewGuid()
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
        var command = new CreateWarehouseCommand
        {
            Name = "Main Warehouse",
            Code = new string('A', 51), // 51 characters
            StreetAddress = "123 Storage St",
            City = "New York",
            PostalCode = "10001",
            CountryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Code" && e.ErrorMessage.Contains("50"));
    }

    [Theory]
    [InlineData("wh-001")] // lowercase
    [InlineData("WH_001")] // underscore
    [InlineData("WH 001")] // space
    [InlineData("WH@001")] // special char
    public void Validate_InvalidCodeFormat_ShouldFail(string code)
    {
        // Arrange
        var command = new CreateWarehouseCommand
        {
            Name = "Main Warehouse",
            Code = code,
            StreetAddress = "123 Storage St",
            City = "New York",
            PostalCode = "10001",
            CountryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Code" && e.ErrorMessage.Contains("uppercase"));
    }

    [Theory]
    [InlineData("WH-001")]
    [InlineData("WAREHOUSE1")]
    [InlineData("WH-MAIN-001")]
    [InlineData("123")]
    public void Validate_ValidCodeFormats_ShouldPass(string code)
    {
        // Arrange
        var command = new CreateWarehouseCommand
        {
            Name = "Main Warehouse",
            Code = code,
            StreetAddress = "123 Storage St",
            City = "New York",
            PostalCode = "10001",
            CountryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_DescriptionTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateWarehouseCommand
        {
            Name = "Main Warehouse",
            Code = "WH-001",
            Description = new string('A', 1001), // 1001 characters
            StreetAddress = "123 Storage St",
            City = "New York",
            PostalCode = "10001",
            CountryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description" && e.ErrorMessage.Contains("1000"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyStreetAddress_ShouldFail(string? streetAddress)
    {
        // Arrange
        var command = new CreateWarehouseCommand
        {
            Name = "Main Warehouse",
            Code = "WH-001",
            StreetAddress = streetAddress!,
            City = "New York",
            PostalCode = "10001",
            CountryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "StreetAddress" && e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyCity_ShouldFail(string? city)
    {
        // Arrange
        var command = new CreateWarehouseCommand
        {
            Name = "Main Warehouse",
            Code = "WH-001",
            StreetAddress = "123 Storage St",
            City = city!,
            PostalCode = "10001",
            CountryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "City" && e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyPostalCode_ShouldFail(string? postalCode)
    {
        // Arrange
        var command = new CreateWarehouseCommand
        {
            Name = "Main Warehouse",
            Code = "WH-001",
            StreetAddress = "123 Storage St",
            City = "New York",
            PostalCode = postalCode!,
            CountryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PostalCode" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_EmptyCountryId_ShouldFail()
    {
        // Arrange
        var command = new CreateWarehouseCommand
        {
            Name = "Main Warehouse",
            Code = "WH-001",
            StreetAddress = "123 Storage St",
            City = "New York",
            PostalCode = "10001",
            CountryId = Guid.Empty
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CountryId" && e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@nodomain.com")]
    [InlineData("missing@")]
    public void Validate_InvalidEmail_ShouldFail(string email)
    {
        // Arrange
        var command = new CreateWarehouseCommand
        {
            Name = "Main Warehouse",
            Code = "WH-001",
            StreetAddress = "123 Storage St",
            City = "New York",
            PostalCode = "10001",
            CountryId = Guid.NewGuid(),
            Email = email
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("email"));
    }

    [Fact]
    public void Validate_ValidEmail_ShouldPass()
    {
        // Arrange
        var command = new CreateWarehouseCommand
        {
            Name = "Main Warehouse",
            Code = "WH-001",
            StreetAddress = "123 Storage St",
            City = "New York",
            PostalCode = "10001",
            CountryId = Guid.NewGuid(),
            Email = "warehouse@example.com"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_NullOptionalFields_ShouldPass()
    {
        // Arrange
        var command = new CreateWarehouseCommand
        {
            Name = "Main Warehouse",
            Code = "WH-001",
            Description = null,
            StreetAddress = "123 Storage St",
            City = "New York",
            State = null,
            PostalCode = "10001",
            CountryId = Guid.NewGuid(),
            Phone = null,
            Email = null,
            SquareFootage = null,
            Capacity = null
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
