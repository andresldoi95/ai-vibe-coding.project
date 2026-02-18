using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Users.Commands.ChangePassword;

/// <summary>
/// Handler for changing current user's password
/// </summary>
public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IAuthService authService,
        ILogger<ChangePasswordCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = request.UserId;
            if (userId == Guid.Empty)
            {
                return Result<bool>.Failure("User not authenticated");
            }

            // Get existing user
            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

            if (user == null || user.IsDeleted)
            {
                return Result<bool>.Failure("User not found");
            }

            // Verify current password
            if (!_authService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                return Result<bool>.Failure("Current password is incorrect");
            }

            // Hash new password
            user.PasswordHash = _authService.HashPassword(request.NewPassword);

            await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} changed their password", userId);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing user password");
            return Result<bool>.Failure("An error occurred while changing the password");
        }
    }
}
