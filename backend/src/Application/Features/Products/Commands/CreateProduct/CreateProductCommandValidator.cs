using FluentValidation;

namespace SaaS.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(256).WithMessage("Product name cannot exceed 256 characters");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Product code is required")
            .MaximumLength(50).WithMessage("Product code cannot exceed 50 characters")
            .Matches("^[A-Z0-9-]+$").WithMessage("Product code can only contain uppercase letters, numbers, and hyphens");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(100).WithMessage("SKU cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Category cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Category));

        RuleFor(x => x.Brand)
            .MaximumLength(100).WithMessage("Brand cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Brand));

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price must be zero or greater");

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Cost price must be zero or greater");

        RuleFor(x => x.MinimumStockLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum stock level must be zero or greater");

        // Validate InitialQuantity (recommended approach for setting initial stock)
        RuleFor(x => x.InitialQuantity)
            .GreaterThan(0).WithMessage("Initial quantity must be greater than 0")
            .When(x => x.InitialQuantity.HasValue);

        // If InitialQuantity is provided, InitialWarehouseId is required
        RuleFor(x => x.InitialWarehouseId)
            .NotEmpty().WithMessage("Initial warehouse is required when initial quantity is specified")
            .When(x => x.InitialQuantity.HasValue && x.InitialQuantity.Value > 0);

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than 0")
            .When(x => x.Weight.HasValue);

        RuleFor(x => x.Dimensions)
            .MaximumLength(100).WithMessage("Dimensions cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Dimensions));
    }
}
