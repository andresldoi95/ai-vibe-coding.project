using FluentValidation;

namespace SaaS.Application.Features.Users.Commands.AcceptInvitation;

/// <summary>
/// Validator for AcceptInvitationCommand
/// </summary>
public class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator()
    {
        RuleFor(x => x.InvitationToken)
            .NotEmpty().WithMessage("Invitation token is required");

        // Name and Password are required for new users, handled in handler business logic
        RuleFor(x => x.Password)
            .MinimumLength(8).When(x => !string.IsNullOrEmpty(x.Password))
            .WithMessage("Password must be at least 8 characters");
    }
}
