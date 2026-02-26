using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.Interfaces;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.CreditNotes.Commands.GenerateCreditNoteRide;

public class GenerateCreditNoteRideCommandHandler : IRequestHandler<GenerateCreditNoteRideCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICreditNoteRideService _creditNoteRideService;
    private readonly ILogger<GenerateCreditNoteRideCommandHandler> _logger;

    public GenerateCreditNoteRideCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ICreditNoteRideService creditNoteRideService,
        ILogger<GenerateCreditNoteRideCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _creditNoteRideService = creditNoteRideService;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(GenerateCreditNoteRideCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            _logger.LogInformation("Generating RIDE for credit note {CreditNoteId}", request.CreditNoteId);

            var creditNote = await _unitOfWork.CreditNotes.GetWithItemsAsync(request.CreditNoteId, tenantId, cancellationToken);
            if (creditNote == null || creditNote.TenantId != tenantId)
                return Result<string>.Failure("Credit note not found");

            if (creditNote.Status != InvoiceStatus.Authorized)
                return Result<string>.Failure($"RIDE can only be generated for authorized credit notes (current status: {creditNote.Status})");

            if (string.IsNullOrEmpty(creditNote.AccessKey))
                return Result<string>.Failure("Access key is required to generate RIDE");

            if (string.IsNullOrEmpty(creditNote.SriAuthorization))
                return Result<string>.Failure("SRI authorization number is required to generate RIDE");

            var sriConfiguration = await _unitOfWork.SriConfigurations.GetByTenantIdAsync(tenantId, cancellationToken);
            if (sriConfiguration == null)
                return Result<string>.Failure("SRI configuration not found");

            if (!creditNote.EmissionPointId.HasValue)
                return Result<string>.Failure("Emission point is required to generate RIDE");

            var emissionPoint = await _unitOfWork.EmissionPoints.GetByIdAsync(creditNote.EmissionPointId.Value, cancellationToken);
            if (emissionPoint == null || emissionPoint.TenantId != tenantId)
                return Result<string>.Failure("Emission point not found");

            var establishment = emissionPoint.Establishment;
            if (establishment == null)
                return Result<string>.Failure("Establishment not found for emission point");

            var rideFilePath = await _creditNoteRideService.GenerateRidePdfAsync(
                creditNote, sriConfiguration, establishment, emissionPoint);

            creditNote.RideFilePath = rideFilePath;
            creditNote.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.CreditNotes.UpdateAsync(creditNote);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("RIDE generated for credit note {CreditNoteId}. Path: {RidePath}", request.CreditNoteId, rideFilePath);

            return Result<string>.Success(rideFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating RIDE for credit note {CreditNoteId}", request.CreditNoteId);
            return Result<string>.Failure("An error occurred while generating the RIDE PDF");
        }
    }
}
