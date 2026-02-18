using FluentValidation;

namespace SaaS.Application.Features.Users.Commands.InviteUser;

/// <summary>
/// Validator for InviteUserCommand
/// </summary>
public class InviteUserCommandValidator : AbstractValidator<InviteUserCommand>
{
    public InviteUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role is required");

        RuleFor(x => x.PersonalMessage)
            .MaximumLength(500).WithMessage("Personal message cannot exceed 500 characters");
    }
}
