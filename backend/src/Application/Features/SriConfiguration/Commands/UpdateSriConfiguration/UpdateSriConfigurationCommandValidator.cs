using FluentValidation;
using SaaS.Domain.Validators;

namespace SaaS.Application.Features.SriConfiguration.Commands.UpdateSriConfiguration;

public class UpdateSriConfigurationCommandValidator : AbstractValidator<UpdateSriConfigurationCommand>
{
    public UpdateSriConfigurationCommandValidator()
    {
        RuleFor(x => x.CompanyRuc)
            .NotEmpty().WithMessage("Company RUC is required")
            .Length(13).WithMessage("Company RUC must be exactly 13 digits")
            .Matches("^[0-9]{13}$").WithMessage("Company RUC must contain only digits")
            .Must(RucValidator.IsValid).WithMessage("Invalid RUC number");

        RuleFor(x => x.LegalName)
            .NotEmpty().WithMessage("Legal name is required")
            .MaximumLength(256).WithMessage("Legal name cannot exceed 256 characters");

        RuleFor(x => x.TradeName)
            .MaximumLength(256).WithMessage("Trade name cannot exceed 256 characters")
            .When(x => !string.IsNullOrEmpty(x.TradeName));

        RuleFor(x => x.MainAddress)
            .NotEmpty().WithMessage("Main address is required")
            .MaximumLength(500).WithMessage("Main address cannot exceed 500 characters");

        RuleFor(x => x.Environment)
            .IsInEnum().WithMessage("Invalid SRI environment");
    }
}
