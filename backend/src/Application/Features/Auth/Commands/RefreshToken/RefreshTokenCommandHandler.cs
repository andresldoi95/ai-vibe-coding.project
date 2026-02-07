using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IUnitOfWork unitOfWork,
        IAuthService authService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            _logger.LogWarning("Invalid refresh token attempt from IP {IpAddress}", request.IpAddress);
            return Result<string>.Failure("Invalid refresh token");
        }

        // Revoke old refresh token and create new one
        await _unitOfWork.RefreshTokens.RevokeTokenAsync(refreshToken, request.IpAddress, cancellationToken);

        var newRefreshToken = _authService.GenerateRefreshToken(request.IpAddress);
        newRefreshToken.UserId = refreshToken.UserId;
        refreshToken.ReplacedByToken = newRefreshToken.Token;

        await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Get user's tenants for JWT
        var user = refreshToken.User;
        var tenants = await _unitOfWork.Tenants.GetUserTenantsAsync(user.Id, cancellationToken);
        var tenantIds = tenants.Select(t => t.Id).ToList();

        // Generate new access token
        var accessToken = _authService.GenerateJwtToken(user, tenantIds);

        _logger.LogInformation("Refresh token renewed for user {UserId}", user.Id);

        return Result<string>.Success(accessToken);
    }
}
