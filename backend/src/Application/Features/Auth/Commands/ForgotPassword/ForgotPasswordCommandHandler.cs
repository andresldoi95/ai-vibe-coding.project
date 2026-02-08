using MediatR;using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using System.Security.Cryptography;

namespace SaaS.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);

        // Always return success to prevent email enumeration attacks
        if (user == null)
        {
            return Result.Success();
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return Result.Success();
        }

        // Generate reset token
        var resetToken = GenerateResetToken();
        user.ResetToken = resetToken;
        user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1); // Token expires in 1 hour

        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send password reset email
        await _emailService.SendPasswordResetAsync(user.Email, resetToken);

        return Result.Success();
    }

    private static string GenerateResetToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}
