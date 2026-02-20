using Xunit;
using FluentAssertions;
using SaaS.Application.Features.Warehouses.Commands.UpdateWarehouse;

namespace Application.Tests.Features.Warehouses.Commands;

public class UpdateWarehouseCommandValidatorTests
{
    private readonly UpdateWarehouseCommandValidator _validator;

    public UpdateWarehouseCommandValidatorTests()
    {
        _validator = new UpdateWarehouseCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new UpdateWarehouseCommand
        {
            Id = Guid.NewGuid(),
            Name = "Updated Warehouse",
            Code = "WH-002",
            Description = "Updated storage facility",
            StreetAddress = "456 Updated St",
            City = "Boston",
            State = "MA",
            PostalCode = "02101",
            CountryId = Guid.NewGuid(),
            Phone = "+1-555-0200",
            Email = "warehouse@example.com",
            SquareFootage = 15000,
            Capacity = 5000
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidCommandWithMinimalData_ShouldPass()
    {
        // Arrange
        var command = new UpdateWarehouseCommand
        {
            Id = Guid.NewGuid(),
            Name = "Minimal Warehouse",
            Code = "WH-MIN",
            StreetAddress = "123 Street",
            City = "City",
            PostalCode = "12345",
            CountryId = Guid.NewGuid()
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
        var command = new UpdateWarehouseCommand
        {
            Id = Guid.Empty,
            Name = "Warehouse",
            Code = "WH-001",
            StreetAddress = "123 Street",
            City = "City",
            PostalCode = "12345",
            CountryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_EmptyName_ShouldFail()
    {
        // Arrange
        var command = new UpdateWarehouseCommand
        {
            Id = Guid.NewGuid(),
            Name = "",
            Code = "WH-001",
            StreetAddress = "123 Street",
            City = "City",
            PostalCode = "12345",
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
        var command = new UpdateWarehouseCommand
        {
            Id = Guid.NewGuid(),
            Name = new string('A', 257),
            Code = "WH-001",
            StreetAddress = "123 Street",
            City = "City",
            PostalCode = "12345",
            CountryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("256"));
    }

    [Fact]
    public void Validate_InvalidCodeFormat_ShouldFail()
    {
        // Arrange
        var command = new UpdateWarehouseCommand
        {
            Id = Guid.NewGuid(),
            Name = "Warehouse",
            Code = "wh-001-lowercase",
            StreetAddress = "123 Street",
            City = "City",
            PostalCode = "12345",
            CountryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Code" && e.ErrorMessage.Contains("uppercase"));
    }

    [Fact]
    public void Validate_DescriptionTooLong_ShouldFail()
    {
        // Arrange
        var command = new UpdateWarehouseCommand
        {
            Id = Guid.NewGuid(),
            Name = "Warehouse",
            Code = "WH-001",
            Description = new string('A', 1001),
            StreetAddress = "123 Street",
            City = "City",
            PostalCode = "12345",
            CountryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description" && e.ErrorMessage.Contains("1000"));
    }

    [Fact]
    public void Validate_StreetAddressTooLong_ShouldFail()
    {
        // Arrange
        var command = new UpdateWarehouseCommand
        {
            Id = Guid.NewGuid(),
            Name = "Warehouse",
            Code = "WH-001",
            StreetAddress = new string('A', 501),
            City = "City",
            PostalCode = "12345",
            CountryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "StreetAddress" && e.ErrorMessage.Contains("500"));
    }

    [Fact]
    public void Validate_EmptyCity_ShouldFail()
    {
        // Arrange
        var command = new UpdateWarehouseCommand
        {
            Id = Guid.NewGuid(),
            Name = "Warehouse",
            Code = "WH-001",
            StreetAddress = "123 Street",
            City = "",
            PostalCode = "12345",
            CountryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "City" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_InvalidEmail_ShouldFail()
    {
        // Arrange
        var command = new UpdateWarehouseCommand
        {
            Id = Guid.NewGuid(),
            Name = "Warehouse",
            Code = "WH-001",
            StreetAddress = "123 Street",
            City = "City",
            PostalCode = "12345",
            CountryId = Guid.NewGuid(),
            Email = "invalid-email"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("Invalid email"));
    }

    [Fact]
    public void Validate_SquareFootageZero_ShouldFail()
    {
        // Arrange
        var command = new UpdateWarehouseCommand
        {
            Id = Guid.NewGuid(),
            Name = "Warehouse",
            Code = "WH-001",
            StreetAddress = "123 Street",
            City = "City",
            PostalCode = "12345",
            CountryId = Guid.NewGuid(),
            SquareFootage = 0
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SquareFootage" && e.ErrorMessage.Contains("greater than 0"));
    }

    [Fact]
    public void Validate_CapacityNegative_ShouldFail()
    {
        // Arrange
        var command = new UpdateWarehouseCommand
        {
            Id = Guid.NewGuid(),
            Name = "Warehouse",
            Code = "WH-001",
            StreetAddress = "123 Street",
            City = "City",
            PostalCode = "12345",
            CountryId = Guid.NewGuid(),
            Capacity = -100
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Capacity" && e.ErrorMessage.Contains("greater than 0"));
    }
}
