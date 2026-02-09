using MediatR;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Permissions.Queries.GetAllPermissions;

public class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, Result<List<PermissionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllPermissionsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<PermissionDto>>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _unitOfWork.Permissions.GetAllAsync(cancellationToken);

        var permissionDtos = permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Resource = p.Resource,
            Action = p.Action
        }).ToList();

        return Result<List<PermissionDto>>.Success(permissionDtos);
    }
}
