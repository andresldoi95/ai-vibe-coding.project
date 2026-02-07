using FluentValidation;

namespace SaaS.Application.Features.Warehouses.Commands.DeleteWarehouse;

public class DeleteWarehouseCommandValidator : AbstractValidator<DeleteWarehouseCommand>
{
    public DeleteWarehouseCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Warehouse ID is required");
    }
}
