using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.CreditNotes.Queries.GetAllCreditNotes;

public class GetAllCreditNotesQueryHandler : IRequestHandler<GetAllCreditNotesQuery, Result<List<CreditNoteDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetAllCreditNotesQueryHandler> _logger;

    public GetAllCreditNotesQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetAllCreditNotesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<List<CreditNoteDto>>> Handle(GetAllCreditNotesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            var allCreditNotes = await _unitOfWork.CreditNotes.GetAllAsync(cancellationToken);
            var creditNotes = allCreditNotes.Where(cn => cn.TenantId == tenantId && !cn.IsDeleted).ToList();

            if (request.CustomerId.HasValue)
                creditNotes = creditNotes.Where(cn => cn.CustomerId == request.CustomerId.Value).ToList();

            if (request.Status.HasValue)
                creditNotes = creditNotes.Where(cn => cn.Status == request.Status.Value).ToList();

            if (request.DateFrom.HasValue)
                creditNotes = creditNotes.Where(cn => cn.IssueDate >= request.DateFrom.Value).ToList();

            if (request.DateTo.HasValue)
                creditNotes = creditNotes.Where(cn => cn.IssueDate <= request.DateTo.Value).ToList();

            // Collect customer names
            var customerIds = creditNotes.Select(cn => cn.CustomerId).Distinct().ToList();
            var customers = new Dictionary<Guid, string>();
            foreach (var customerId in customerIds)
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(customerId, cancellationToken);
                if (customer != null)
                    customers[customerId] = customer.Name;
            }

            // Collect emission point data
            var emissionPointIds = creditNotes
                .Where(cn => cn.EmissionPointId.HasValue)
                .Select(cn => cn.EmissionPointId!.Value)
                .Distinct()
                .ToList();

            var emissionPointData = new Dictionary<Guid, (string Code, string Name, string EstablishmentCode)>();
            foreach (var epId in emissionPointIds)
            {
                var ep = await _unitOfWork.EmissionPoints.GetByIdAsync(epId, cancellationToken);
                if (ep != null && ep.Establishment != null)
                    emissionPointData[epId] = (ep.EmissionPointCode, ep.Name, ep.Establishment.EstablishmentCode);
            }

            var dtos = creditNotes
                .OrderByDescending(cn => cn.IssueDate)
                .Select(cn => new CreditNoteDto
                {
                    Id = cn.Id,
                    TenantId = cn.TenantId,
                    CreditNoteNumber = cn.CreditNoteNumber,
                    CustomerId = cn.CustomerId,
                    CustomerName = customers.GetValueOrDefault(cn.CustomerId, string.Empty),
                    EmissionPointId = cn.EmissionPointId,
                    EmissionPointCode = cn.EmissionPointId.HasValue && emissionPointData.ContainsKey(cn.EmissionPointId.Value)
                        ? emissionPointData[cn.EmissionPointId.Value].Code : null,
                    EmissionPointName = cn.EmissionPointId.HasValue && emissionPointData.ContainsKey(cn.EmissionPointId.Value)
                        ? emissionPointData[cn.EmissionPointId.Value].Name : null,
                    EstablishmentCode = cn.EmissionPointId.HasValue && emissionPointData.ContainsKey(cn.EmissionPointId.Value)
                        ? emissionPointData[cn.EmissionPointId.Value].EstablishmentCode : null,
                    IssueDate = cn.IssueDate,
                    Status = cn.Status,
                    SubtotalAmount = cn.SubtotalAmount,
                    TaxAmount = cn.TaxAmount,
                    TotalAmount = cn.TotalAmount,
                    ValueModification = cn.ValueModification,
                    OriginalInvoiceId = cn.OriginalInvoiceId,
                    OriginalInvoiceNumber = cn.OriginalInvoiceNumber,
                    OriginalInvoiceDate = cn.OriginalInvoiceDate,
                    Reason = cn.Reason,
                    Notes = cn.Notes,
                    DocumentType = cn.DocumentType,
                    AccessKey = cn.AccessKey,
                    PaymentMethod = cn.PaymentMethod,
                    XmlFilePath = cn.XmlFilePath,
                    SignedXmlFilePath = cn.SignedXmlFilePath,
                    RideFilePath = cn.RideFilePath,
                    Environment = cn.Environment,
                    SriAuthorization = cn.SriAuthorization,
                    AuthorizationDate = cn.AuthorizationDate,
                    CreatedAt = cn.CreatedAt,
                    UpdatedAt = cn.UpdatedAt,
                    IsEditable = cn.IsEditable
                })
                .ToList();

            return Result<List<CreditNoteDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting credit notes");
            return Result<List<CreditNoteDto>>.Failure("An error occurred while retrieving credit notes");
        }
    }
}
