using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.CreditNotes.Commands.UpdateCreditNote;

public class UpdateCreditNoteCommandHandler : IRequestHandler<UpdateCreditNoteCommand, Result<CreditNoteDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ITaxCalculationService _taxCalculationService;
    private readonly ILogger<UpdateCreditNoteCommandHandler> _logger;

    public UpdateCreditNoteCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ITaxCalculationService taxCalculationService,
        ILogger<UpdateCreditNoteCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _taxCalculationService = taxCalculationService;
        _logger = logger;
    }

    public async Task<Result<CreditNoteDto>> Handle(UpdateCreditNoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var creditNote = await _unitOfWork.CreditNotes.GetWithItemsAsync(request.Id, tenantId, cancellationToken);
            if (creditNote == null || creditNote.TenantId != tenantId)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<CreditNoteDto>.Failure("Credit note not found");
            }

            if (!creditNote.IsEditable)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<CreditNoteDto>.Failure("Credit note cannot be edited. Only draft credit notes can be modified.");
            }

            var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer == null || customer.TenantId != tenantId)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<CreditNoteDto>.Failure("Customer not found");
            }

            creditNote.CustomerId = request.CustomerId;
            creditNote.Reason = request.Reason;
            creditNote.Notes = request.Notes;

            if (request.IssueDate.HasValue)
                creditNote.IssueDate = DateTime.SpecifyKind(request.IssueDate.Value, DateTimeKind.Utc);

            // Delete items no longer in request
            var updatedItemIds = request.Items.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();
            var itemsToDelete = creditNote.Items.Where(i => !updatedItemIds.Contains(i.Id)).ToList();
            foreach (var item in itemsToDelete)
                await _unitOfWork.CreditNoteItems.DeleteAsync(item, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Process new/updated items
            var updatedItems = new List<CreditNoteItem>();

            foreach (var itemDto in request.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId, cancellationToken);
                if (product == null || product.TenantId != tenantId)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<CreditNoteDto>.Failure("Product not found");
                }

                var taxRate = await _unitOfWork.TaxRates.GetByIdAsync(itemDto.TaxRateId, cancellationToken);
                if (taxRate == null || taxRate.TenantId != tenantId)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<CreditNoteDto>.Failure("Tax rate not found");
                }

                var (subtotal, tax, total) = _taxCalculationService.CalculateLineItemTotals(
                    itemDto.Quantity, product.UnitPrice, taxRate.Rate);

                if (itemDto.Id.HasValue)
                {
                    // Update existing item
                    var existingItem = creditNote.Items.FirstOrDefault(i => i.Id == itemDto.Id.Value);
                    if (existingItem != null)
                    {
                        existingItem.ProductId = product.Id;
                        existingItem.ProductCode = product.Code;
                        existingItem.ProductName = product.Name;
                        existingItem.Description = itemDto.Description;
                        existingItem.Quantity = itemDto.Quantity;
                        existingItem.UnitPrice = product.UnitPrice;
                        existingItem.TaxRateId = taxRate.Id;
                        existingItem.TaxRate = taxRate.Rate;
                        existingItem.SubtotalAmount = subtotal;
                        existingItem.TaxAmount = tax;
                        existingItem.TotalAmount = total;
                        await _unitOfWork.CreditNoteItems.UpdateAsync(existingItem, cancellationToken);
                        updatedItems.Add(existingItem);
                    }
                }
                else
                {
                    // Add new item
                    var newItem = new CreditNoteItem
                    {
                        CreditNoteId = creditNote.Id,
                        ProductId = product.Id,
                        ProductCode = product.Code,
                        ProductName = product.Name,
                        Description = itemDto.Description,
                        Quantity = itemDto.Quantity,
                        UnitPrice = product.UnitPrice,
                        TaxRateId = taxRate.Id,
                        TaxRate = taxRate.Rate,
                        SubtotalAmount = subtotal,
                        TaxAmount = tax,
                        TotalAmount = total
                    };
                    await _unitOfWork.CreditNoteItems.AddAsync(newItem, cancellationToken);
                    updatedItems.Add(newItem);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Recalculate totals
            var allItems = updatedItems;
            var calcDtos = allItems.Select(ii => new InvoiceItemDto
                { Quantity = ii.Quantity, UnitPrice = ii.UnitPrice, TaxRate = ii.TaxRate }).ToList();

            var (newSubtotal, newTax, newTotal) = _taxCalculationService.CalculateInvoiceTotals(calcDtos);

            creditNote.SubtotalAmount = newSubtotal;
            creditNote.TaxAmount = newTax;
            creditNote.TotalAmount = newTotal;
            creditNote.ValueModification = newTotal;
            creditNote.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.CreditNotes.UpdateAsync(creditNote);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Credit note {Id} updated successfully", creditNote.Id);

            // Re-fetch with full navigation data
            var updated = await _unitOfWork.CreditNotes.GetWithItemsAsync(creditNote.Id, tenantId, cancellationToken);

            return Result<CreditNoteDto>.Success(MapToDto(updated!, customer.Name));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating credit note {Id}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<CreditNoteDto>.Failure("An error occurred while updating the credit note");
        }
    }

    private static CreditNoteDto MapToDto(CreditNote cn, string customerName) => new()
    {
        Id = cn.Id,
        TenantId = cn.TenantId,
        CreditNoteNumber = cn.CreditNoteNumber,
        CustomerId = cn.CustomerId,
        CustomerName = customerName,
        EmissionPointId = cn.EmissionPointId,
        EmissionPointCode = cn.EmissionPoint?.EmissionPointCode,
        EmissionPointName = cn.EmissionPoint?.Name,
        EstablishmentCode = cn.EmissionPoint?.Establishment?.EstablishmentCode,
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
        IsPhysicalReturn = cn.IsPhysicalReturn,
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
        IsEditable = cn.IsEditable,
        Items = cn.Items.Select(ii => new CreditNoteItemDto
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
}
