using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Entities;

namespace SaaS.Application.Features.Users.Commands.AcceptInvitation;

/// <summary>
/// Handler for accepting a user invitation
/// </summary>
public class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, Result<LoginResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly ILogger<AcceptInvitationCommandHandler> _logger;

    public AcceptInvitationCommandHandler(
        IUnitOfWork unitOfWork,
        IAuthService authService,
        ILogger<AcceptInvitationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _logger = logger;
    }

    public async Task<Result<LoginResponseDto>> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get invitation
            var invitation = await _unitOfWork.UserInvitations.GetByTokenAsync(request.InvitationToken, cancellationToken);

            if (invitation == null)
            {
                return Result<LoginResponseDto>.Failure("Invalid invitation token");
            }

            if (!invitation.IsActive)
            {
                return Result<LoginResponseDto>.Failure("This invitation has already been used");
            }

            if (invitation.ExpiresAt < DateTime.UtcNow)
            {
                return Result<LoginResponseDto>.Failure("This invitation has expired");
            }

            if (invitation.AcceptedAt.HasValue)
            {
                return Result<LoginResponseDto>.Failure("This invitation has already been accepted");
            }

            // Get user
            var user = await _unitOfWork.Users.GetByEmailAsync(invitation.Email, cancellationToken);

            if (user == null)
            {
                return Result<LoginResponseDto>.Failure("User not found");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // If user is new (IsActive = false), set their name and password
            if (!user.IsActive)
            {
                if (string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Password))
                {
                    return Result<LoginResponseDto>.Failure("Name and password are required for new users");
                }

                user.Name = request.Name;
                user.PasswordHash = _authService.HashPassword(request.Password);
                user.IsActive = true;
                user.EmailConfirmed = true;

                await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // Check if UserTenant already exists (in case user was previously removed)
            var existingUserTenant = await _unitOfWork.UserTenants.GetByUserAndTenantAsync(
                user.Id,
                invitation.TenantId,
                cancellationToken);

            if (existingUserTenant != null)
            {
                // Reactivate existing membership
                existingUserTenant.IsActive = true;
                existingUserTenant.RoleId = invitation.RoleId;
                existingUserTenant.JoinedAt = DateTime.UtcNow;

                await _unitOfWork.UserTenants.UpdateAsync(existingUserTenant, cancellationToken);
            }
            else
            {
                // Create new UserTenant association
                var userTenant = new UserTenant
                {
                    UserId = user.Id,
                    TenantId = invitation.TenantId,
                    RoleId = invitation.RoleId,
                    IsActive = true,
                    JoinedAt = DateTime.UtcNow
                };

                await _unitOfWork.UserTenants.AddAsync(userTenant, cancellationToken);
            }

            // Mark invitation as accepted
            invitation.AcceptedAt = DateTime.UtcNow;
            invitation.IsActive = false;

            await _unitOfWork.UserInvitations.UpdateAsync(invitation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Generate JWT and refresh tokens
            var userTenants = await _unitOfWork.Tenants.GetUserTenantsAsync(user.Id);
            var tenantIds = userTenants.Select(t => t.Id).ToList();
            var accessToken = _authService.GenerateJwtToken(user, tenantIds);
            var refreshToken = _authService.GenerateRefreshToken("127.0.0.1");
            refreshToken.UserId = user.Id;

            await _unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("User {Email} accepted invitation to tenant {TenantId}", user.Email, invitation.TenantId);

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
                Tenants = userTenants.Select(t => new TenantDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Slug = t.Slug,
                    Status = t.Status.ToString()
                }).ToList()
            };

            return Result<LoginResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error accepting invitation with token {Token}", request.InvitationToken);
            return Result<LoginResponseDto>.Failure("Failed to accept invitation. Please try again.");
        }
    }
}
