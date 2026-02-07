using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Auth.Commands.Login;

/// <summary>
/// Handler for user login
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IAuthService authService,
        ILogger<LoginCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _logger = logger;
    }

    public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Validate credentials
        var user = await _authService.ValidateCredentialsAsync(request.Email, request.Password);
        if (user == null)
        {
            _logger.LogWarning("Failed login attempt for email {Email}", request.Email);
            return Result<LoginResponseDto>.Failure("Invalid email or password");
        }

        // Get user's tenants
        var tenants = await _unitOfWork.Tenants.GetUserTenantsAsync(user.Id, cancellationToken);
        var tenantIds = tenants.Select(t => t.Id).ToList();

        // Generate tokens
        var accessToken = _authService.GenerateJwtToken(user, tenantIds);
        var refreshToken = _authService.GenerateRefreshToken(request.IpAddress);
        refreshToken.UserId = user.Id;

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {Email} logged in successfully", request.Email);

        // Prepare response
        var response = new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed
            },
            Tenants = tenants.Select(t => new TenantDto
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                Status = t.Status.ToString()
            }).ToList()
        };

        return Result<LoginResponseDto>.Success(response);
    }
}
