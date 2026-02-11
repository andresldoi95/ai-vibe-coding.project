using FluentValidation;

namespace SaaS.Application.Features.Establishments.Commands.DeleteEstablishment;

public class DeleteEstablishmentCommandValidator : AbstractValidator<DeleteEstablishmentCommand>
{
    public DeleteEstablishmentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Establishment ID is required");
    }
}
