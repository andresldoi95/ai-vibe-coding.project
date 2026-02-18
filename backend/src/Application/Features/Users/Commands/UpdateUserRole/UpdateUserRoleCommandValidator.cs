using FluentValidation;

namespace SaaS.Application.Features.Users.Commands.UpdateUserRole;

/// <summary>
/// Validator for UpdateUserRoleCommand
/// </summary>
public class UpdateUserRoleCommandValidator : AbstractValidator<UpdateUserRoleCommand>
{
    public UpdateUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.NewRoleId)
            .NotEmpty().WithMessage("Role ID is required");
    }
}
