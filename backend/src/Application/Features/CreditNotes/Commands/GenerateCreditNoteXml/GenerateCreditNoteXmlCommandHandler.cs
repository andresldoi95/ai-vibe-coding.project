using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.Interfaces;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.CreditNotes.Commands.GenerateCreditNoteXml;

public class GenerateCreditNoteXmlCommandHandler : IRequestHandler<GenerateCreditNoteXmlCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICreditNoteXmlService _creditNoteXmlService;
    private readonly ILogger<GenerateCreditNoteXmlCommandHandler> _logger;

    public GenerateCreditNoteXmlCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ICreditNoteXmlService creditNoteXmlService,
        ILogger<GenerateCreditNoteXmlCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _creditNoteXmlService = creditNoteXmlService;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(GenerateCreditNoteXmlCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            _logger.LogInformation("Generating XML for credit note {CreditNoteId}", request.CreditNoteId);

            var creditNote = await _unitOfWork.CreditNotes.GetWithItemsAsync(request.CreditNoteId, tenantId, cancellationToken);
            if (creditNote == null || creditNote.TenantId != tenantId)
                return Result<string>.Failure("Credit note not found");

            var allowedStatuses = new[] { InvoiceStatus.Draft, InvoiceStatus.PendingSignature, InvoiceStatus.PendingAuthorization };
            if (!allowedStatuses.Contains(creditNote.Status))
                return Result<string>.Failure($"Cannot generate XML for credit note with status: {creditNote.Status}");

            var sriConfiguration = await _unitOfWork.SriConfigurations.GetByTenantIdAsync(tenantId, cancellationToken);
            if (sriConfiguration == null)
                return Result<string>.Failure("SRI configuration not found. Please configure SRI settings first.");

            if (!creditNote.EmissionPointId.HasValue)
                return Result<string>.Failure("Credit note must have an emission point to generate XML");

            var emissionPoint = await _unitOfWork.EmissionPoints.GetByIdAsync(creditNote.EmissionPointId.Value, cancellationToken);
            if (emissionPoint == null || emissionPoint.TenantId != tenantId)
                return Result<string>.Failure("Emission point not found");

            var establishment = emissionPoint.Establishment;
            if (establishment == null)
                return Result<string>.Failure("Establishment not found for emission point");

            var (xmlFilePath, accessKey) = await _creditNoteXmlService.GenerateCreditNoteXmlAsync(
                creditNote, sriConfiguration, establishment, emissionPoint);

            creditNote.XmlFilePath = xmlFilePath;
            creditNote.AccessKey = accessKey;
            creditNote.Status = InvoiceStatus.PendingSignature;
            creditNote.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.CreditNotes.UpdateAsync(creditNote);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("XML generated for credit note {CreditNoteId}. Path: {XmlFilePath}", request.CreditNoteId, xmlFilePath);

            return Result<string>.Success(xmlFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating XML for credit note {CreditNoteId}", request.CreditNoteId);
            return Result<string>.Failure("An error occurred while generating the XML");
        }
    }
}
