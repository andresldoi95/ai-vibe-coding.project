using MediatR;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    public ResetPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // Find user by reset token
        var user = await _unitOfWork.Users.GetByResetTokenAsync(request.Token, cancellationToken);

        if (user == null)
        {
            return Result.Failure("Invalid or expired reset token");
        }

        // Check if token has expired
        if (user.ResetTokenExpiry == null || user.ResetTokenExpiry < DateTime.UtcNow)
        {
            return Result.Failure("Reset token has expired");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return Result.Failure("User account is inactive");
        }

        // Hash new password
        user.PasswordHash = _authService.HashPassword(request.NewPassword);

        // Clear reset token
        user.ResetToken = null;
        user.ResetTokenExpiry = null;

        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
