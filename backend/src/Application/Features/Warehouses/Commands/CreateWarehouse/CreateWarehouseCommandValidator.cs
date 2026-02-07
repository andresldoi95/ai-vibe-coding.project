using FluentValidation;

namespace SaaS.Application.Features.Warehouses.Commands.CreateWarehouse;

public class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Warehouse name is required")
            .MaximumLength(256).WithMessage("Warehouse name cannot exceed 256 characters");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Warehouse code is required")
            .MaximumLength(50).WithMessage("Warehouse code cannot exceed 50 characters")
            .Matches("^[A-Z0-9-]+$").WithMessage("Warehouse code can only contain uppercase letters, numbers, and hyphens");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.StreetAddress)
            .NotEmpty().WithMessage("Street address is required")
            .MaximumLength(500).WithMessage("Street address cannot exceed 500 characters");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters");

        RuleFor(x => x.State)
            .MaximumLength(100).WithMessage("State cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.State));

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("Postal code is required")
            .MaximumLength(20).WithMessage("Postal code cannot exceed 20 characters");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required")
            .MaximumLength(100).WithMessage("Country cannot exceed 100 characters");

        RuleFor(x => x.Phone)
            .MaximumLength(50).WithMessage("Phone cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.SquareFootage)
            .GreaterThan(0).WithMessage("Square footage must be greater than 0")
            .When(x => x.SquareFootage.HasValue);

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than 0")
            .When(x => x.Capacity.HasValue);
    }
}
