using FluentValidation;

namespace SaaS.Application.Features.InvoiceConfigurations.Commands.UpdateInvoiceConfiguration;

public class UpdateInvoiceConfigurationCommandValidator : AbstractValidator<UpdateInvoiceConfigurationCommand>
{
    public UpdateInvoiceConfigurationCommandValidator()
    {
        RuleFor(x => x.EstablishmentCode)
            .NotEmpty().WithMessage("Establishment code is required")
            .Length(3).WithMessage("Establishment code must be exactly 3 characters")
            .Matches("^[0-9]+$").WithMessage("Establishment code must contain only digits");

        RuleFor(x => x.EmissionPointCode)
            .NotEmpty().WithMessage("Emission point code is required")
            .Length(3).WithMessage("Emission point code must be exactly 3 characters")
            .Matches("^[0-9]+$").WithMessage("Emission point code must contain only digits");

        RuleFor(x => x.DueDays)
            .GreaterThan(0).WithMessage("Due days must be greater than 0");
    }
}
