using FluentValidation;

namespace SaaS.Application.Features.StockMovements.Commands.DeleteStockMovement;

/// <summary>
/// Validator for DeleteStockMovementCommand
/// </summary>
public class DeleteStockMovementCommandValidator : AbstractValidator<DeleteStockMovementCommand>
{
    public DeleteStockMovementCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Stock movement ID is required");
    }
}
