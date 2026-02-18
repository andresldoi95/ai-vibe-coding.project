using FluentValidation;

namespace SaaS.Application.Features.Users.Commands.RemoveUserFromCompany;

/// <summary>
/// Validator for RemoveUserFromCompanyCommand
/// </summary>
public class RemoveUserFromCompanyCommandValidator : AbstractValidator<RemoveUserFromCompanyCommand>
{
    public RemoveUserFromCompanyCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
