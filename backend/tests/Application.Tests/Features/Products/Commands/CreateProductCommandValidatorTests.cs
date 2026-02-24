using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using SaaS.Application.Features.Products.Commands.CreateProduct;

namespace Application.Tests.Features.Products.Commands;

public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator;

    public CreateProductCommandValidatorTests()
    {
        _validator = new CreateProductCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateProductCommand
        {
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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyName_ShouldFail(string? name)
    {
        // Arrange
        var command = new CreateProductCommand
        {
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
        var command = new CreateProductCommand
        {
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
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyCode_ShouldFail(string? code)
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Code = code!,
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
            .WithErrorMessage("Product code is required");
    }

    [Fact]
    public void Validate_CodeTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Code = new string('A', 26),
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
            .WithErrorMessage("Product code cannot exceed 25 characters (SRI XML schema limit for codigoPrincipal)");
    }

    [Theory]
    [InlineData("prod-001")]
    [InlineData("PROD_001")]
    [InlineData("PROD 001")]
    [InlineData("PROD@001")]
    public void Validate_InvalidCodeFormat_ShouldFail(string code)
    {
        // Arrange
        var command = new CreateProductCommand
        {
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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptySKU_ShouldFail(string? sku)
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Code = "PROD-001",
            SKU = sku!,
            UnitPrice = 99.99m,
            CostPrice = 50.00m,
            MinimumStockLevel = 10,
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SKU)
            .WithErrorMessage("SKU is required");
    }

    [Fact]
    public void Validate_SKUTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Code = "PROD-001",
            SKU = new string('A', 101),
            UnitPrice = 99.99m,
            CostPrice = 50.00m,
            MinimumStockLevel = 10,
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SKU)
            .WithErrorMessage("SKU cannot exceed 100 characters");
    }

    [Fact]
    public void Validate_NegativeUnitPrice_ShouldFail()
    {
        // Arrange
        var command = new CreateProductCommand
        {
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
        var command = new CreateProductCommand
        {
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

    [Fact]
    public void Validate_NegativeMinimumStockLevel_ShouldFail()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Code = "PROD-001",
            SKU = "SKU-001",
            UnitPrice = 99.99m,
            CostPrice = 50.00m,
            MinimumStockLevel = -5,
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MinimumStockLevel)
            .WithErrorMessage("Minimum stock level must be zero or greater");
    }

    [Fact]
    public void Validate_InvalidInitialQuantity_ShouldFail()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Code = "PROD-001",
            SKU = "SKU-001",
            UnitPrice = 99.99m,
            CostPrice = 50.00m,
            MinimumStockLevel = 10,
            InitialQuantity = 0,
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.InitialQuantity)
            .WithErrorMessage("Initial quantity must be greater than 0");
    }

    [Fact]
    public void Validate_InitialQuantityWithoutWarehouse_ShouldFail()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Code = "PROD-001",
            SKU = "SKU-001",
            UnitPrice = 99.99m,
            CostPrice = 50.00m,
            MinimumStockLevel = 10,
            InitialQuantity = 100,
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.InitialWarehouseId)
            .WithErrorMessage("Initial warehouse is required when initial quantity is specified");
    }

    [Fact]
    public void Validate_InitialQuantityWithWarehouse_ShouldPass()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Code = "PROD-001",
            SKU = "SKU-001",
            UnitPrice = 99.99m,
            CostPrice = 50.00m,
            MinimumStockLevel = 10,
            InitialQuantity = 100,
            InitialWarehouseId = Guid.NewGuid(),
            IsActive = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.InitialQuantity);
        result.ShouldNotHaveValidationErrorFor(x => x.InitialWarehouseId);
    }
}
