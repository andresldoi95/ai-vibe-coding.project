using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.Interfaces;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.Invoices.Commands.GenerateInvoiceXml;

public class GenerateInvoiceXmlCommandHandler : IRequestHandler<GenerateInvoiceXmlCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IInvoiceXmlService _invoiceXmlService;
    private readonly ILogger<GenerateInvoiceXmlCommandHandler> _logger;

    public GenerateInvoiceXmlCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        IInvoiceXmlService invoiceXmlService,
        ILogger<GenerateInvoiceXmlCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _invoiceXmlService = invoiceXmlService;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(GenerateInvoiceXmlCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            _logger.LogInformation("Generating XML for invoice {InvoiceId}", request.InvoiceId);

            // Get invoice with all required relationships
            var invoice = await _unitOfWork.Invoices.GetWithItemsAsync(request.InvoiceId, tenantId, cancellationToken);
            if (invoice == null || invoice.TenantId != tenantId)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found for tenant {TenantId}", request.InvoiceId, tenantId);
                return Result<string>.Failure("Invoice not found");
            }

            // Validate invoice status - allow re-generation from any non-final status
            var allowedStatuses = new[] { InvoiceStatus.Draft, InvoiceStatus.PendingSignature, InvoiceStatus.PendingAuthorization };
            if (!allowedStatuses.Contains(invoice.Status))
            {
                _logger.LogWarning("Invoice {InvoiceId} cannot regenerate XML from status {Status}", request.InvoiceId, invoice.Status);
                return Result<string>.Failure($"Cannot generate XML for invoice with status: {invoice.Status}");
            }

            // Get SRI configuration
            var sriConfiguration = await _unitOfWork.SriConfigurations.GetByTenantIdAsync(tenantId, cancellationToken);
            if (sriConfiguration == null)
            {
                _logger.LogWarning("SRI configuration not found for tenant {TenantId}", tenantId);
                return Result<string>.Failure("SRI configuration not found. Please configure SRI settings first.");
            }

            // Validate emission point ID exists
            if (!invoice.EmissionPointId.HasValue)
            {
                _logger.LogWarning("Invoice {InvoiceId} has no emission point", request.InvoiceId);
                return Result<string>.Failure("Invoice must have an emission point to generate XML");
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

            // Generate XML file
            var (xmlFilePath, accessKey) = await _invoiceXmlService.GenerateInvoiceXmlAsync(
                invoice,
                sriConfiguration,
                establishment,
                emissionPoint);

            // Update invoice with XML path, access key, and change status to PendingSignature
            invoice.XmlFilePath = xmlFilePath;
            invoice.AccessKey = accessKey;
            invoice.Status = InvoiceStatus.PendingSignature;
            invoice.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Invoices.UpdateAsync(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "XML generated successfully for invoice {InvoiceId}. Path: {XmlFilePath}",
                request.InvoiceId,
                xmlFilePath);

            return Result<string>.Success(xmlFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating XML for invoice {InvoiceId}", request.InvoiceId);

            // Log error to database
            try
            {
                var tenantId = _tenantContext.TenantId ?? Guid.Empty;
                var errorLog = new Domain.Entities.SriErrorLog
                {
                    InvoiceId = request.InvoiceId,
                    TenantId = tenantId,
                    Operation = "GenerateXml",
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

            return Result<string>.Failure($"Error generating XML: {ex.Message}");
        }
    }
}
