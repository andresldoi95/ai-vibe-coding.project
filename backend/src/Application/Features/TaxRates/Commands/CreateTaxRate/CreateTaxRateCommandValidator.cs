using FluentValidation;

namespace SaaS.Application.Features.TaxRates.Commands.CreateTaxRate;

public class CreateTaxRateCommandValidator : AbstractValidator<CreateTaxRateCommand>
{
    public CreateTaxRateCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Rate)
            .GreaterThanOrEqualTo(0).WithMessage("Rate must be greater than or equal to 0")
            .LessThanOrEqualTo(1).WithMessage("Rate must be less than or equal to 1");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.Country)
            .MaximumLength(2).WithMessage("Country must be a 2-character ISO code");
    }
}
