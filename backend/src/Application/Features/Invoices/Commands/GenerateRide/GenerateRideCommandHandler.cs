using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.Interfaces;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.Invoices.Commands.GenerateRide;

public class GenerateRideCommandHandler : IRequestHandler<GenerateRideCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IRideGenerationService _rideGenerationService;
    private readonly ILogger<GenerateRideCommandHandler> _logger;

    public GenerateRideCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        IRideGenerationService rideGenerationService,
        ILogger<GenerateRideCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _rideGenerationService = rideGenerationService;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(GenerateRideCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            _logger.LogInformation("Generating RIDE PDF for invoice {InvoiceId}", request.InvoiceId);

            // Get invoice with all required relationships
            var invoice = await _unitOfWork.Invoices.GetWithItemsAsync(request.InvoiceId, tenantId, cancellationToken);
            if (invoice == null || invoice.TenantId != tenantId)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found for tenant {TenantId}", request.InvoiceId, tenantId);
                return Result<string>.Failure("Invoice not found");
            }

            // Validate invoice status - must be authorized
            if (invoice.Status != InvoiceStatus.Authorized)
            {
                _logger.LogWarning("Invoice {InvoiceId} is not authorized (current status: {Status})", request.InvoiceId, invoice.Status);
                return Result<string>.Failure($"RIDE can only be generated for authorized invoices (current status: {invoice.Status})");
            }

            // Validate required fields
            if (string.IsNullOrEmpty(invoice.AccessKey))
            {
                _logger.LogWarning("Invoice {InvoiceId} has no access key", request.InvoiceId);
                return Result<string>.Failure("Access key is required to generate RIDE");
            }

            if (string.IsNullOrEmpty(invoice.SriAuthorization))
            {
                _logger.LogWarning("Invoice {InvoiceId} has no SRI authorization", request.InvoiceId);
                return Result<string>.Failure("SRI authorization number is required to generate RIDE");
            }

            // Get SRI configuration
            var sriConfiguration = await _unitOfWork.SriConfigurations.GetByTenantIdAsync(tenantId, cancellationToken);
            if (sriConfiguration == null)
            {
                _logger.LogWarning("SRI configuration not found for tenant {TenantId}", tenantId);
                return Result<string>.Failure("SRI configuration not found");
            }

            // Validate emission point ID exists
            if (!invoice.EmissionPointId.HasValue)
            {
                _logger.LogWarning("Invoice {InvoiceId} has no emission point", request.InvoiceId);
                return Result<string>.Failure("Emission point is required to generate RIDE");
            }

            // Get emission point with establishment
            var emissionPoint = await _unitOfWork.EmissionPoints.GetByIdAsync(invoice.EmissionPointId.Value, cancellationToken);
            if (emissionPoint == null || emissionPoint.TenantId != tenantId)
            {
                _logger.LogWarning("Emission point {EmissionPointId} not found for tenant {TenantId}", invoice.EmissionPointId, tenantId);
                return Result<string>.Failure("Emission point not found");
            }

            var establishment = emissionPoint.Establishment;
            if (establishment == null)
            {
                _logger.LogWarning("Establishment not found for emission point {EmissionPointId}", invoice.EmissionPointId);
                return Result<string>.Failure("Establishment not found for emission point");
            }

            // Generate RIDE PDF
            var rideFilePath = await _rideGenerationService.GenerateRidePdfAsync(
                invoice,
                sriConfiguration,
                establishment,
                emissionPoint);

            // Update invoice with RIDE file path
            invoice.RideFilePath = rideFilePath;
            invoice.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Invoices.UpdateAsync(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "RIDE PDF generated successfully for invoice {InvoiceId}. Path: {RideFilePath}",
                request.InvoiceId,
                rideFilePath);

            return Result<string>.Success(rideFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating RIDE PDF for invoice {InvoiceId}", request.InvoiceId);

            // Log error to database
            try
            {
                var tenantId = _tenantContext.TenantId ?? Guid.Empty;
                var errorLog = new Domain.Entities.SriErrorLog
                {
                    InvoiceId = request.InvoiceId,
                    TenantId = tenantId,
                    Operation = "GenerateRIDE",
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                    OccurredAt = DateTime.UtcNow
                };
                await _unitOfWork.SriErrorLogs.AddAsync(errorLog);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Failed to log SRI error for invoice {InvoiceId}", request.InvoiceId);
            }

            return Result<string>.Failure($"Error generating RIDE PDF: {ex.Message}");
        }
    }
}
