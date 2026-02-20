using FluentValidation;

namespace SaaS.Application.Features.EmissionPoints.Commands.CreateEmissionPoint;

public class CreateEmissionPointCommandValidator : AbstractValidator<CreateEmissionPointCommand>
{
    public CreateEmissionPointCommandValidator()
    {
        RuleFor(x => x.EstablishmentId)
            .NotEmpty().WithMessage("Establishment ID is required");

        RuleFor(x => x.EmissionPointCode)
            .NotEmpty().WithMessage("Emission point code is required")
            .Length(3).WithMessage("Emission point code must be exactly 3 digits")
            .Matches("^[0-9]{3}$").WithMessage("Emission point code must be exactly 3 digits (001-999)")
            .Must(code => string.IsNullOrWhiteSpace(code) || !int.TryParse(code, out var number) || (number >= 1 && number <= 999))
            .WithMessage("Emission point code must be between 001 and 999");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Emission point name is required")
            .MaximumLength(256).WithMessage("Emission point name cannot exceed 256 characters");
    }
}
