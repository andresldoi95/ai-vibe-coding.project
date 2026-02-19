using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using SaaS.Application.Features.Products.Commands.UpdateProduct;

namespace Application.Tests.Features.Products.Commands;

public class UpdateProductCommandValidatorTests
{
    private readonly UpdateProductCommandValidator _validator;

    public UpdateProductCommandValidatorTests()
    {
        _validator = new UpdateProductCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Code = "PROD-001",
            SKU = "SKU-001",
            Description = "Test description",
            Category = "Electronics",
            Brand = "TestBrand",
            UnitPrice = 99.99m,
            CostPrice = 50.00m,
            MinimumStockLevel = 10,
            Weight = 1.5m,
            Dimensions = "10x10x10",
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyId_ShouldFail()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.Empty,
            Name = "Test Product",
            Code = "PROD-001",
            SKU = "SKU-001",
            UnitPrice = 99.99m,
            CostPrice = 50.00m,
            MinimumStockLevel = 10,
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Product ID is required");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyName_ShouldFail(string? name)
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = name!,
            Code = "PROD-001",
            SKU = "SKU-001",
            UnitPrice = 99.99m,
            CostPrice = 50.00m,
            MinimumStockLevel = 10,
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Product name is required");
    }

    [Fact]
    public void Validate_NameTooLong_ShouldFail()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = new string('a', 257),
            Code = "PROD-001",
            SKU = "SKU-001",
            UnitPrice = 99.99m,
            CostPrice = 50.00m,
            MinimumStockLevel = 10,
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Product name cannot exceed 256 characters");
    }

    [Theory]
    [InlineData("prod-001")]
    [InlineData("PROD_001")]
    [InlineData("PROD 001")]
    public void Validate_InvalidCodeFormat_ShouldFail(string code)
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Code = code,
            SKU = "SKU-001",
            UnitPrice = 99.99m,
            CostPrice = 50.00m,
            MinimumStockLevel = 10,
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Product code can only contain uppercase letters, numbers, and hyphens");
    }

    [Fact]
    public void Validate_NegativeUnitPrice_ShouldFail()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Code = "PROD-001",
            SKU = "SKU-001",
            UnitPrice = -10m,
            CostPrice = 50.00m,
            MinimumStockLevel = 10,
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UnitPrice)
            .WithErrorMessage("Unit price must be zero or greater");
    }

    [Fact]
    public void Validate_NegativeCostPrice_ShouldFail()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Code = "PROD-001",
            SKU = "SKU-001",
            UnitPrice = 99.99m,
            CostPrice = -10m,
            MinimumStockLevel = 10,
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CostPrice)
            .WithErrorMessage("Cost price must be zero or greater");
    }
}
