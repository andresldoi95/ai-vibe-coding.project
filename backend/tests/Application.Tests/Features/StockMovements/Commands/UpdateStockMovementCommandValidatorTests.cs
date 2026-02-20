using Xunit;
using FluentAssertions;
using SaaS.Application.Features.StockMovements.Commands.UpdateStockMovement;
using SaaS.Domain.Enums;

namespace Application.Tests.Features.StockMovements.Commands;

public class UpdateStockMovementCommandValidatorTests
{
    private readonly UpdateStockMovementCommandValidator _validator;

    public UpdateStockMovementCommandValidatorTests()
    {
        _validator = new UpdateStockMovementCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new UpdateStockMovementCommand
        {
            Id = Guid.NewGuid(),
            MovementType = MovementType.Purchase,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            Quantity = 100,
            MovementDate = DateTime.UtcNow,
            UnitCost = 10.50m,
            TotalCost = 1050.00m,
            Reference = "PO-12345",
            Notes = "Stock replenishment"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidTransferCommand_ShouldPass()
    {
        // Arrange
        var command = new UpdateStockMovementCommand
        {
            Id = Guid.NewGuid(),
            MovementType = MovementType.Transfer,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            DestinationWarehouseId = Guid.NewGuid(),
            Quantity = 50,
            MovementDate = DateTime.UtcNow
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
        var command = new UpdateStockMovementCommand
        {
            Id = Guid.Empty,
            MovementType = MovementType.Purchase,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            Quantity = 100,
            MovementDate = DateTime.UtcNow
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_InvalidMovementType_ShouldFail()
    {
        // Arrange
        var command = new UpdateStockMovementCommand
        {
            Id = Guid.NewGuid(),
            MovementType = (MovementType)999,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            Quantity = 100,
            MovementDate = DateTime.UtcNow
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MovementType" && e.ErrorMessage.Contains("Invalid movement type"));
    }

    [Fact]
    public void Validate_EmptyProductId_ShouldFail()
    {
        // Arrange
        var command = new UpdateStockMovementCommand
        {
            Id = Guid.NewGuid(),
            MovementType = MovementType.Purchase,
            ProductId = Guid.Empty,
            WarehouseId = Guid.NewGuid(),
            Quantity = 100,
            MovementDate = DateTime.UtcNow
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProductId" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_EmptyWarehouseId_ShouldFail()
    {
        // Arrange
        var command = new UpdateStockMovementCommand
        {
            Id = Guid.NewGuid(),
            MovementType = MovementType.Purchase,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.Empty,
            Quantity = 100,
            MovementDate = DateTime.UtcNow
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "WarehouseId" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_QuantityZero_ShouldFail()
    {
        // Arrange
        var command = new UpdateStockMovementCommand
        {
            Id = Guid.NewGuid(),
            MovementType = MovementType.Purchase,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            Quantity = 0,
            MovementDate = DateTime.UtcNow
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Quantity" && e.ErrorMessage.Contains("cannot be zero"));
    }

    [Fact]
    public void Validate_TransferWithoutDestination_ShouldFail()
    {
        // Arrange
        var command = new UpdateStockMovementCommand
        {
            Id = Guid.NewGuid(),
            MovementType = MovementType.Transfer,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            Quantity = 50,
            MovementDate = DateTime.UtcNow
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DestinationWarehouseId" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_TransferWithSameDestination_ShouldFail()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var command = new UpdateStockMovementCommand
        {
            Id = Guid.NewGuid(),
            MovementType = MovementType.Transfer,
            ProductId = Guid.NewGuid(),
            WarehouseId = warehouseId,
            DestinationWarehouseId = warehouseId,
            Quantity = 50,
            MovementDate = DateTime.UtcNow
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DestinationWarehouseId" && e.ErrorMessage.Contains("different"));
    }

    [Fact]
    public void Validate_NegativeUnitCost_ShouldFail()
    {
        // Arrange
        var command = new UpdateStockMovementCommand
        {
            Id = Guid.NewGuid(),
            MovementType = MovementType.Purchase,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            Quantity = 100,
            MovementDate = DateTime.UtcNow,
            UnitCost = -10.00m
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UnitCost" && e.ErrorMessage.Contains("greater than or equal to zero"));
    }

    [Fact]
    public void Validate_NegativeTotalCost_ShouldFail()
    {
        // Arrange
        var command = new UpdateStockMovementCommand
        {
            Id = Guid.NewGuid(),
            MovementType = MovementType.Purchase,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            Quantity = 100,
            MovementDate = DateTime.UtcNow,
            TotalCost = -500.00m
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TotalCost" && e.ErrorMessage.Contains("greater than or equal to zero"));
    }

    [Fact]
    public void Validate_ReferenceTooLong_ShouldFail()
    {
        // Arrange
        var command = new UpdateStockMovementCommand
        {
            Id = Guid.NewGuid(),
            MovementType = MovementType.Purchase,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            Quantity = 100,
            MovementDate = DateTime.UtcNow,
            Reference = new string('A', 101)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Reference" && e.ErrorMessage.Contains("100"));
    }

    [Fact]
    public void Validate_NotesTooLong_ShouldFail()
    {
        // Arrange
        var command = new UpdateStockMovementCommand
        {
            Id = Guid.NewGuid(),
            MovementType = MovementType.Purchase,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            Quantity = 100,
            MovementDate = DateTime.UtcNow,
            Notes = new string('A', 501)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Notes" && e.ErrorMessage.Contains("500"));
    }

    [Fact]
    public void Validate_EmptyMovementDate_ShouldFail()
    {
        // Arrange
        var command = new UpdateStockMovementCommand
        {
            Id = Guid.NewGuid(),
            MovementType = MovementType.Purchase,
            ProductId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            Quantity = 100,
            MovementDate = default
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MovementDate" && e.ErrorMessage.Contains("required"));
    }
}
