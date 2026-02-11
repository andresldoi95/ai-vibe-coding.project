using FluentValidation;

namespace SaaS.Application.Features.EmissionPoints.Commands.DeleteEmissionPoint;

public class DeleteEmissionPointCommandValidator : AbstractValidator<DeleteEmissionPointCommand>
{
    public DeleteEmissionPointCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Emission point ID is required");
    }
}
