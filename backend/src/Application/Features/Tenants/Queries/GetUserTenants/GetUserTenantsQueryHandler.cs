using MediatR;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Tenants.Queries.GetUserTenants;

public class GetUserTenantsQueryHandler : IRequestHandler<GetUserTenantsQuery, Result<List<TenantDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserTenantsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<TenantDto>>> Handle(GetUserTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenants = await _unitOfWork.Tenants.GetUserTenantsAsync(request.UserId, cancellationToken);

        var tenantDtos = tenants.Select(t => new TenantDto
        {
            Id = t.Id,
            Name = t.Name,
            Slug = t.Slug,
            Status = t.Status.ToString()
        }).ToList();

        return Result<List<TenantDto>>.Success(tenantDtos);
    }
}
