using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.EmissionPoints.Commands.DeleteEmissionPoint;

/// <summary>
/// Handler for deleting an emission point
/// </summary>
public class DeleteEmissionPointCommandHandler : IRequestHandler<DeleteEmissionPointCommand, Result<Unit>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<DeleteEmissionPointCommandHandler> _logger;

    public DeleteEmissionPointCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<DeleteEmissionPointCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(DeleteEmissionPointCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<Unit>.Failure("Tenant context is required");
            }

            var emissionPoint = await _unitOfWork.EmissionPoints.GetByIdAsync(request.Id, cancellationToken);

            if (emissionPoint == null)
            {
                return Result<Unit>.Failure("Emission point not found");
            }

            if (emissionPoint.TenantId != _tenantContext.TenantId.Value)
            {
                return Result<Unit>.Failure("Emission point not found");
            }

            await _unitOfWork.EmissionPoints.DeleteAsync(emissionPoint, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Emission point {Code} deleted successfully for tenant {TenantId}",
                emissionPoint.EmissionPointCode,
                emissionPoint.TenantId);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting emission point {Id} for tenant {TenantId}", request.Id, _tenantContext.TenantId);
            return Result<Unit>.Failure($"Failed to delete emission point: {ex.Message}");
        }
    }
}
