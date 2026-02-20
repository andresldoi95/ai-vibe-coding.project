using FluentValidation;

namespace SaaS.Application.Features.Establishments.Commands.UpdateEstablishment;

public class UpdateEstablishmentCommandValidator : AbstractValidator<UpdateEstablishmentCommand>
{
    public UpdateEstablishmentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Establishment ID is required");

        RuleFor(x => x.EstablishmentCode)
            .NotEmpty().WithMessage("Establishment code is required")
            .Length(3).WithMessage("Establishment code must be exactly 3 digits")
            .Matches("^[0-9]{3}$").WithMessage("Establishment code must be exactly 3 digits (001-999)")
            .Must(code => string.IsNullOrWhiteSpace(code) || !int.TryParse(code, out var number) || (number >= 1 && number <= 999))
            .WithMessage("Establishment code must be between 001 and 999");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Establishment name is required")
            .MaximumLength(256).WithMessage("Establishment name cannot exceed 256 characters");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.Phone)
            .MaximumLength(50).WithMessage("Phone cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}
