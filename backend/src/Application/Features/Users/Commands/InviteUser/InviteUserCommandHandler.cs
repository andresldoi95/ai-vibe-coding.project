using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Domain.Entities;

namespace SaaS.Application.Features.Users.Commands.InviteUser;

/// <summary>
/// Handler for inviting a user to the current company
/// </summary>
public class InviteUserCommandHandler : IRequestHandler<InviteUserCommand, Result<Unit>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IEmailService _emailService;
    private readonly ILogger<InviteUserCommandHandler> _logger;

    public InviteUserCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        IEmailService emailService,
        ILogger<InviteUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(InviteUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<Unit>.Failure("Tenant context is required");
            }

            if (!_tenantContext.UserId.HasValue)
            {
                return Result<Unit>.Failure("User context is required");
            }

            var invitedByUserId = _tenantContext.UserId.Value;

            // Get tenant
            var tenant = await _unitOfWork.Tenants.GetByIdAsync(_tenantContext.TenantId.Value, cancellationToken);
            if (tenant == null)
            {
                return Result<Unit>.Failure("Tenant not found");
            }

            // Verify role exists and belongs to current tenant
            var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId, cancellationToken);
            if (role == null || role.TenantId != _tenantContext.TenantId.Value)
            {
                return Result<Unit>.Failure("Invalid role selected");
            }

            // Check if user already exists globally
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);

            // Check if user is already in this company
            if (existingUser != null)
            {
                var existingUserTenant = await _unitOfWork.UserTenants.GetByUserAndTenantAsync(
                    existingUser.Id,
                    _tenantContext.TenantId.Value,
                    cancellationToken);

                if (existingUserTenant != null && existingUserTenant.IsActive)
                {
                    return Result<Unit>.Failure("User already exists in this company");
                }
            }

            // Check if there's already an active invitation
            var existingInvitation = await _unitOfWork.UserInvitations.GetActiveByEmailAndTenantAsync(
                request.Email,
                _tenantContext.TenantId.Value,
                cancellationToken);

            if (existingInvitation != null)
            {
                return Result<Unit>.Failure("An active invitation already exists for this email");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // Create user if they don't exist
            if (existingUser == null)
            {
                existingUser = new User
                {
                    Email = request.Email,
                    Name = string.Empty, // Will be set when they accept invitation
                    PasswordHash = string.Empty, // Will be set when they accept invitation
                    IsActive = false, // Activated when invitation is accepted
                    EmailConfirmed = false
                };

                await _unitOfWork.Users.AddAsync(existingUser, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // Generate invitation token
            var invitationToken = Guid.NewGuid().ToString();

            // Create invitation
            var invitation = new UserInvitation
            {
                InvitationToken = invitationToken,
                Email = request.Email,
                TenantId = _tenantContext.TenantId.Value,
                RoleId = request.RoleId,
                InvitedByUserId = invitedByUserId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(48),
                IsActive = true
            };

            await _unitOfWork.UserInvitations.AddAsync(invitation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send invitation email
            var emailResult = await _emailService.SendUserInvitationAsync(
                request.Email,
                tenant.Name,
                invitationToken,
                _tenantContext.TenantId.Value);

            if (!emailResult.IsSuccess)
            {
                _logger.LogWarning("Failed to send invitation email to {Email}: {Error}", request.Email, emailResult.Error);
                // Continue anyway - invitation is created
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("User {Email} invited to tenant {TenantId} by user {InvitedBy}",
                request.Email, _tenantContext.TenantId.Value, invitedByUserId);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error inviting user {Email}", request.Email);
            return Result<Unit>.Failure("Failed to send invitation. Please try again.");
        }
    }
}
