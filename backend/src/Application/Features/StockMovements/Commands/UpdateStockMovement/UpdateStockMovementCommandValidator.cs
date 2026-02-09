using FluentValidation;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.StockMovements.Commands.UpdateStockMovement;

/// <summary>
/// Validator for UpdateStockMovementCommand
/// </summary>
public class UpdateStockMovementCommandValidator : AbstractValidator<UpdateStockMovementCommand>
{
    public UpdateStockMovementCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Stock movement ID is required");

        RuleFor(x => x.MovementType)
            .IsInEnum()
            .WithMessage("Invalid movement type");

        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product is required");

        RuleFor(x => x.WarehouseId)
            .NotEmpty()
            .WithMessage("Warehouse is required");

        RuleFor(x => x.Quantity)
            .NotEqual(0)
            .WithMessage("Quantity cannot be zero");

        RuleFor(x => x.DestinationWarehouseId)
            .NotEmpty()
            .When(x => x.MovementType == MovementType.Transfer)
            .WithMessage("Destination warehouse is required for transfer movements");

        RuleFor(x => x.DestinationWarehouseId)
            .NotEqual(x => x.WarehouseId)
            .When(x => x.MovementType == MovementType.Transfer && x.DestinationWarehouseId.HasValue)
            .WithMessage("Destination warehouse must be different from source warehouse");

        RuleFor(x => x.UnitCost)
            .GreaterThanOrEqualTo(0)
            .When(x => x.UnitCost.HasValue)
            .WithMessage("Unit cost must be greater than or equal to zero");

        RuleFor(x => x.TotalCost)
            .GreaterThanOrEqualTo(0)
            .When(x => x.TotalCost.HasValue)
            .WithMessage("Total cost must be greater than or equal to zero");

        RuleFor(x => x.Reference)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Reference))
            .WithMessage("Reference must not exceed 100 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes must not exceed 500 characters");

        RuleFor(x => x.MovementDate)
            .NotEmpty()
            .WithMessage("Movement date is required");
    }
}
