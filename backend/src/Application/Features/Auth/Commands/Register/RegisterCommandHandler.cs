using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.Auth.Commands.Register;

/// <summary>
/// Handler for company + user registration
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<LoginResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUnitOfWork unitOfWork,
        IAuthService authService,
        ILogger<RegisterCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _logger = logger;
    }

    public async Task<Result<LoginResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if email already exists
            if (await _unitOfWork.Users.EmailExistsAsync(request.Email, cancellationToken))
            {
                return Result<LoginResponseDto>.Failure("Email already registered");
            }

            // Check if slug already exists
            if (await _unitOfWork.Tenants.SlugExistsAsync(request.Slug, cancellationToken))
            {
                return Result<LoginResponseDto>.Failure("Company slug already taken");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // Create tenant (company)
            var tenant = new Tenant
            {
                Name = request.CompanyName,
                Slug = request.Slug,
                Status = TenantStatus.Active,
                SchemaName = $"tenant_{request.Slug}"
            };

            await _unitOfWork.Tenants.AddAsync(tenant, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Create user
            var user = new User
            {
                Email = request.Email,
                Name = request.Name,
                PasswordHash = _authService.HashPassword(request.Password),
                IsActive = true,
                EmailConfirmed = false
            };

            await _unitOfWork.Users.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Create user-tenant relationship with Owner role
            var userTenant = new UserTenant
            {
                UserId = user.Id,
                TenantId = tenant.Id,
                Role = UserRole.Owner,
                IsActive = true
            };

            await _unitOfWork.UserTenants.AddAsync(userTenant, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Generate JWT and refresh tokens
            var tenantIds = new List<Guid> { tenant.Id };
            var accessToken = _authService.GenerateJwtToken(user, tenantIds);
            var refreshToken = _authService.GenerateRefreshToken("127.0.0.1");
            refreshToken.UserId = user.Id;

            await _unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Successfully registered user {Email} and company {CompanyName}", request.Email, request.CompanyName);

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
                Tenants = new List<TenantDto>
                {
                    new TenantDto
                    {
                        Id = tenant.Id,
                        Name = tenant.Name,
                        Slug = tenant.Slug,
                        Status = tenant.Status.ToString()
                    }
                }
            };

            return Result<LoginResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error registering user {Email}", request.Email);
            return Result<LoginResponseDto>.Failure("Registration failed. Please try again.");
        }
    }
}
