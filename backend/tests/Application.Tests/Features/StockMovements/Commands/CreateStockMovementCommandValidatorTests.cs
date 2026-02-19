using Xunit;
using FluentAssertions;
using SaaS.Application.Features.StockMovements.Commands.CreateStockMovement;
using SaaS.Domain.Enums;

namespace Application.Tests.Features.StockMovements.Commands;

public class CreateStockMovementCommandValidatorTests
{
    private readonly CreateStockMovementCommandValidator _validator;

    public CreateStockMovementCommandValidatorTests()
    {
        _validator = new CreateStockMovementCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateStockMovementCommand
        {
            MovementType = MovementType.Purchase,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            Quantity = 100,
            UnitCost = 10.50m,
            Reference = "PO-001",
            Notes = "Initial purchase"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyProductId_ShouldFail()
    {
        // Arrange
        var command = new CreateStockMovementCommand
        {
            MovementType = MovementType.Purchase,
            ProductId = Guid.Empty,
            WarehouseId = Guid.NewGuid(),
            Quantity = 100
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProductId");
    }

    [Fact]
    public void Validate_EmptyWarehouseId_ShouldFail()
    {
        // Arrange
        var command = new CreateStockMovementCommand
        {
            MovementType = MovementType.Purchase,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.Empty,
            Quantity = 100
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "WarehouseId");
    }

    [Fact]
    public void Validate_ZeroQuantity_ShouldFail()
    {
        // Arrange
        var command = new CreateStockMovementCommand
        {
            MovementType = MovementType.Purchase,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            Quantity = 0
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Quantity" &&
            e.ErrorMessage.Contains("cannot be zero"));
    }

    [Fact]
    public void Validate_TransferWithoutDestination_ShouldFail()
    {
        // Arrange
        var command = new CreateStockMovementCommand
        {
            MovementType = MovementType.Transfer,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            DestinationWarehouseId = null,
            Quantity = 50
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "DestinationWarehouseId" &&
            e.ErrorMessage.Contains("required for transfer"));
    }

    [Fact]
    public void Validate_TransferSameWarehouse_ShouldFail()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var command = new CreateStockMovementCommand
        {
            MovementType = MovementType.Transfer,
            ProductId = Guid.NewGuid(),
            WarehouseId = warehouseId,
            DestinationWarehouseId = warehouseId,
            Quantity = 50
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "DestinationWarehouseId" &&
            e.ErrorMessage.Contains("must be different"));
    }

    [Fact]
    public void Validate_NegativeUnitCost_ShouldFail()
    {
        // Arrange
        var command = new CreateStockMovementCommand
        {
            MovementType = MovementType.Purchase,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            Quantity = 100,
            UnitCost = -10.50m
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UnitCost");
    }

    [Fact]
    public void Validate_NegativeTotalCost_ShouldFail()
    {
        // Arrange
        var command = new CreateStockMovementCommand
        {
            MovementType = MovementType.Purchase,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            Quantity = 100,
            TotalCost = -100.00m
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TotalCost");
    }

    [Theory]
    [InlineData(101)]
    [InlineData(150)]
    [InlineData(200)]
    public void Validate_ReferenceTooLong_ShouldFail(int length)
    {
        // Arrange
        var command = new CreateStockMovementCommand
        {
            MovementType = MovementType.Purchase,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            Quantity = 100,
            Reference = new string('A', length)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Reference" &&
            e.ErrorMessage.Contains("100 characters"));
    }

    [Theory]
    [InlineData(501)]
    [InlineData(600)]
    [InlineData(1000)]
    public void Validate_NotesTooLong_ShouldFail(int length)
    {
        // Arrange
        var command = new CreateStockMovementCommand
        {
            MovementType = MovementType.Purchase,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            Quantity = 100,
            Notes = new string('A', length)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Notes" &&
            e.ErrorMessage.Contains("500 characters"));
    }

    [Fact]
    public void Validate_ValidReferenceLength_ShouldPass()
    {
        // Arrange
        var command = new CreateStockMovementCommand
        {
            MovementType = MovementType.Purchase,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            Quantity = 100,
            Reference = new string('A', 100)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ValidNotesLength_ShouldPass()
    {
        // Arrange
        var command = new CreateStockMovementCommand
        {
            MovementType = MovementType.Purchase,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            Quantity = 100,
            Notes = new string('A', 500)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
