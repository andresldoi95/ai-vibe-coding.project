using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Auth.Commands.SelectTenant;

/// <summary>
/// Command to select a tenant and generate tenant-scoped JWT with role/permissions
/// </summary>
public class SelectTenantCommand : IRequest<Result<SelectTenantResponseDto>>
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
}
