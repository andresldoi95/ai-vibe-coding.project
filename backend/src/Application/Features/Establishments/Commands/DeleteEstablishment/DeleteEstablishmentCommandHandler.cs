using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Establishments.Commands.DeleteEstablishment;

/// <summary>
/// Handler for deleting an establishment
/// </summary>
public class DeleteEstablishmentCommandHandler : IRequestHandler<DeleteEstablishmentCommand, Result<Unit>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<DeleteEstablishmentCommandHandler> _logger;

    public DeleteEstablishmentCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<DeleteEstablishmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(DeleteEstablishmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<Unit>.Failure("Tenant context is required");
            }

            var establishment = await _unitOfWork.Establishments.GetByIdAsync(request.Id, cancellationToken);

            if (establishment == null)
            {
                return Result<Unit>.Failure("Establishment not found");
            }

            if (establishment.TenantId != _tenantContext.TenantId.Value)
            {
                return Result<Unit>.Failure("Establishment not found");
            }

            await _unitOfWork.Establishments.DeleteAsync(establishment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Establishment {Code} deleted successfully for tenant {TenantId}",
                establishment.EstablishmentCode,
                establishment.TenantId);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting establishment {Id} for tenant {TenantId}", request.Id, _tenantContext.TenantId);
            return Result<Unit>.Failure($"Failed to delete establishment: {ex.Message}");
        }
    }
}
