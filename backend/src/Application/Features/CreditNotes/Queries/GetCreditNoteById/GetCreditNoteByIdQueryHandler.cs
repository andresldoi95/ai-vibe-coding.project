using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.CreditNotes.Queries.GetCreditNoteById;

public class GetCreditNoteByIdQueryHandler : IRequestHandler<GetCreditNoteByIdQuery, Result<CreditNoteDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetCreditNoteByIdQueryHandler> _logger;

    public GetCreditNoteByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetCreditNoteByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<CreditNoteDto>> Handle(GetCreditNoteByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            var creditNote = await _unitOfWork.CreditNotes.GetWithItemsAsync(request.Id, tenantId, cancellationToken);
            if (creditNote == null)
                return Result<CreditNoteDto>.Failure("Credit note not found");

            var dto = new CreditNoteDto
            {
                Id = creditNote.Id,
                TenantId = creditNote.TenantId,
                CreditNoteNumber = creditNote.CreditNoteNumber,
                CustomerId = creditNote.CustomerId,
                CustomerName = creditNote.Customer?.Name ?? string.Empty,
                EmissionPointId = creditNote.EmissionPointId,
                EmissionPointCode = creditNote.EmissionPoint?.EmissionPointCode,
                EmissionPointName = creditNote.EmissionPoint?.Name,
                EstablishmentCode = creditNote.EmissionPoint?.Establishment?.EstablishmentCode,
                IssueDate = creditNote.IssueDate,
                Status = creditNote.Status,
                SubtotalAmount = creditNote.SubtotalAmount,
                TaxAmount = creditNote.TaxAmount,
                TotalAmount = creditNote.TotalAmount,
                ValueModification = creditNote.ValueModification,
                OriginalInvoiceId = creditNote.OriginalInvoiceId,
                OriginalInvoiceNumber = creditNote.OriginalInvoiceNumber,
                OriginalInvoiceDate = creditNote.OriginalInvoiceDate,
                Reason = creditNote.Reason,
                Notes = creditNote.Notes,
                DocumentType = creditNote.DocumentType,
                AccessKey = creditNote.AccessKey,
                PaymentMethod = creditNote.PaymentMethod,
                XmlFilePath = creditNote.XmlFilePath,
                SignedXmlFilePath = creditNote.SignedXmlFilePath,
                RideFilePath = creditNote.RideFilePath,
                Environment = creditNote.Environment,
                SriAuthorization = creditNote.SriAuthorization,
                AuthorizationDate = creditNote.AuthorizationDate,
                CreatedAt = creditNote.CreatedAt,
                UpdatedAt = creditNote.UpdatedAt,
                IsEditable = creditNote.IsEditable,
                Items = creditNote.Items.Select(ii => new CreditNoteItemDto
                {
                    Id = ii.Id,
                    CreditNoteId = ii.CreditNoteId,
                    ProductId = ii.ProductId,
                    ProductCode = ii.ProductCode,
                    ProductName = ii.ProductName,
                    Description = ii.Description,
                    Quantity = ii.Quantity,
                    UnitPrice = ii.UnitPrice,
                    TaxRateId = ii.TaxRateId,
                    TaxRate = ii.TaxRate,
                    SubtotalAmount = ii.SubtotalAmount,
                    TaxAmount = ii.TaxAmount,
                    TotalAmount = ii.TotalAmount
                }).ToList()
            };

            return Result<CreditNoteDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving credit note {CreditNoteId}", request.Id);
            return Result<CreditNoteDto>.Failure("An error occurred while retrieving the credit note");
        }
    }
}
