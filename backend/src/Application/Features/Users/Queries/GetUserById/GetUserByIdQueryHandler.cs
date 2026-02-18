using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Users.Queries.GetUserById;

/// <summary>
/// Handler for retrieving a specific user by ID in the current company
/// </summary>
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<CompanyUserDto>>
{
    private readonly IUserTenantRepository _userTenantRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    public GetUserByIdQueryHandler(
        IUserTenantRepository userTenantRepository,
        ITenantContext tenantContext,
        ILogger<GetUserByIdQueryHandler> logger)
    {
        _userTenantRepository = userTenantRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<CompanyUserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<CompanyUserDto>.Failure("Tenant context is required");
            }

            var userTenant = await _userTenantRepository.GetByUserAndTenantAsync(
                request.UserId,
                _tenantContext.TenantId.Value,
                cancellationToken);

            if (userTenant == null)
            {
                return Result<CompanyUserDto>.Failure("User not found in this company");
            }

            var companyUser = new CompanyUserDto
            {
                Id = userTenant.UserId,
                Name = userTenant.User.Name,
                Email = userTenant.User.Email,
                Role = new RoleDto
                {
                    Id = userTenant.Role!.Id,
                    Name = userTenant.Role.Name,
                    Description = userTenant.Role.Description,
                    Priority = userTenant.Role.Priority
                },
                JoinedAt = userTenant.JoinedAt,
                IsActive = userTenant.IsActive
            };

            return Result<CompanyUserDto>.Success(companyUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", request.UserId);
            return Result<CompanyUserDto>.Failure("Failed to retrieve user");
        }
    }
}
