using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Users.Queries.GetCompanyUsers;

/// <summary>
/// Handler for retrieving all users in the current company
/// </summary>
public class GetCompanyUsersQueryHandler : IRequestHandler<GetCompanyUsersQuery, Result<List<CompanyUserDto>>>
{
    private readonly IUserTenantRepository _userTenantRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetCompanyUsersQueryHandler> _logger;

    public GetCompanyUsersQueryHandler(
        IUserTenantRepository userTenantRepository,
        ITenantContext tenantContext,
        ILogger<GetCompanyUsersQueryHandler> logger)
    {
        _userTenantRepository = userTenantRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<List<CompanyUserDto>>> Handle(GetCompanyUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<List<CompanyUserDto>>.Failure("Tenant context is required");
            }

            var userTenants = await _userTenantRepository.GetByTenantIdWithDetailsAsync(
                _tenantContext.TenantId.Value,
                cancellationToken);

            var companyUsers = userTenants.Select(ut => new CompanyUserDto
            {
                Id = ut.UserId,
                Name = ut.User.Name,
                Email = ut.User.Email,
                Role = new RoleDto
                {
                    Id = ut.Role!.Id,
                    Name = ut.Role.Name,
                    Description = ut.Role.Description,
                    Priority = ut.Role.Priority
                },
                JoinedAt = ut.JoinedAt,
                IsActive = ut.IsActive
            }).ToList();

            _logger.LogInformation("Retrieved {Count} users for tenant {TenantId}", companyUsers.Count, _tenantContext.TenantId.Value);

            return Result<List<CompanyUserDto>>.Success(companyUsers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving company users");
            return Result<List<CompanyUserDto>>.Failure("Failed to retrieve company users");
        }
    }
}
