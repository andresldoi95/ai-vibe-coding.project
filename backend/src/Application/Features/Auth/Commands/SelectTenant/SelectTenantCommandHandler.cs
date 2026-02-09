using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Auth.Commands.SelectTenant;

/// <summary>
/// Handler for selecting a tenant and generating tenant-scoped JWT
/// </summary>
public class SelectTenantCommandHandler : IRequestHandler<SelectTenantCommand, Result<SelectTenantResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly ILogger<SelectTenantCommandHandler> _logger;

    public SelectTenantCommandHandler(
        IUnitOfWork unitOfWork,
        IAuthService authService,
        ILogger<SelectTenantCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _logger = logger;
    }

    public async Task<Result<SelectTenantResponseDto>> Handle(SelectTenantCommand request, CancellationToken cancellationToken)
    {
        // Get user
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result<SelectTenantResponseDto>.Failure("User not found");
        }

        // Verify user has access to this tenant and get role with permissions
        var userTenant = await _unitOfWork.UserTenants.GetWithRoleAndPermissionsAsync(
            request.UserId,
            request.TenantId,
            cancellationToken);

        if (userTenant == null)
        {
            _logger.LogWarning("User {UserId} attempted to select unauthorized tenant {TenantId}",
                request.UserId, request.TenantId);
            return Result<SelectTenantResponseDto>.Failure("Access denied to this tenant");
        }

        if (userTenant.Role == null || userTenant.RoleId == null)
        {
            _logger.LogError("User {UserId} has no role assigned for tenant {TenantId}",
                request.UserId, request.TenantId);
            return Result<SelectTenantResponseDto>.Failure("No role assigned. Please contact your administrator.");
        }

        // Extract permissions
        var permissions = userTenant.Role.RolePermissions
            .Select(rp => rp.Permission.Name)
            .ToList();

        // Generate tenant-scoped JWT with role and permissions
        var accessToken = _authService.GenerateJwtTokenWithRole(
            user,
            request.TenantId,
            userTenant.Role,
            permissions);

        _logger.LogInformation("User {Email} selected tenant {TenantId} with role {Role}",
            user.Email, request.TenantId, userTenant.Role.Name);

        var response = new SelectTenantResponseDto
        {
            AccessToken = accessToken,
            Role = new RoleDto
            {
                Id = userTenant.Role.Id,
                Name = userTenant.Role.Name,
                Description = userTenant.Role.Description,
                Priority = userTenant.Role.Priority
            },
            Permissions = permissions
        };

        return Result<SelectTenantResponseDto>.Success(response);
    }
}
