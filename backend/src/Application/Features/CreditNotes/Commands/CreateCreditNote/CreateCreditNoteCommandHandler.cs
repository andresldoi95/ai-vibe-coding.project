using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.CreditNotes.Commands.CreateCreditNote;

public class CreateCreditNoteCommandHandler : IRequestHandler<CreateCreditNoteCommand, Result<CreditNoteDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ITaxCalculationService _taxCalculationService;
    private readonly ILogger<CreateCreditNoteCommandHandler> _logger;

    public CreateCreditNoteCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ITaxCalculationService taxCalculationService,
        ILogger<CreateCreditNoteCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _taxCalculationService = taxCalculationService;
        _logger = logger;
    }

    public async Task<Result<CreditNoteDto>> Handle(CreateCreditNoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            _logger.LogInformation("Handling CreateCreditNoteCommand for tenant {TenantId}", tenantId);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // Validate customer exists
            var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer == null || customer.TenantId != tenantId)
            {
                _logger.LogWarning("Customer {CustomerId} not found for tenant {TenantId}", request.CustomerId, tenantId);
                return Result<CreditNoteDto>.Failure("Customer not found");
            }

            // Validate original invoice — must be Authorized and belong to the same tenant
            var originalInvoice = await _unitOfWork.Invoices.GetWithItemsAsync(request.OriginalInvoiceId, tenantId, cancellationToken);
            if (originalInvoice == null || originalInvoice.TenantId != tenantId)
            {
                _logger.LogWarning("Original invoice {InvoiceId} not found for tenant {TenantId}", request.OriginalInvoiceId, tenantId);
                return Result<CreditNoteDto>.Failure("Original invoice not found");
            }

            if (originalInvoice.Status != InvoiceStatus.Authorized)
            {
                _logger.LogWarning("Original invoice {InvoiceId} is not authorized (status: {Status})", request.OriginalInvoiceId, originalInvoice.Status);
                return Result<CreditNoteDto>.Failure("Credit notes can only be issued against authorized invoices");
            }

            // Validate customer matches original invoice
            if (originalInvoice.CustomerId != request.CustomerId)
            {
                _logger.LogWarning("Customer mismatch: credit note customer {CustomerId} does not match invoice customer {InvoiceCustomerId}",
                    request.CustomerId, originalInvoice.CustomerId);
                return Result<CreditNoteDto>.Failure("Customer does not match the original invoice");
            }

            // Validate emission point
            var emissionPoint = await _unitOfWork.EmissionPoints.GetByIdAsync(request.EmissionPointId, cancellationToken);
            if (emissionPoint == null || emissionPoint.TenantId != tenantId)
            {
                _logger.LogWarning("Emission point {EmissionPointId} not found for tenant {TenantId}", request.EmissionPointId, tenantId);
                return Result<CreditNoteDto>.Failure("Emission point not found");
            }

            if (!emissionPoint.IsActive)
            {
                _logger.LogWarning("Emission point {EmissionPointId} is not active", request.EmissionPointId);
                return Result<CreditNoteDto>.Failure("Emission point is not active");
            }

            if (emissionPoint.Establishment == null)
            {
                _logger.LogWarning("Establishment not found for emission point {EmissionPointId}", request.EmissionPointId);
                return Result<CreditNoteDto>.Failure("Establishment not found for emission point");
            }

            var establishment = emissionPoint.Establishment;

            // Increment CreditNote sequence atomically
            emissionPoint.CreditNoteSequence++;
            var sequentialNumber = emissionPoint.CreditNoteSequence;
            await _unitOfWork.EmissionPoints.UpdateAsync(emissionPoint);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var creditNoteNumber = $"{establishment.EstablishmentCode}-{emissionPoint.EmissionPointCode}-{sequentialNumber:D9}";

            // Get SRI configuration
            var config = await _unitOfWork.SriConfigurations.GetByTenantIdAsync(tenantId, cancellationToken);
            if (config == null)
            {
                _logger.LogWarning("SRI configuration not found for tenant {TenantId}", tenantId);
                return Result<CreditNoteDto>.Failure("SRI configuration not found");
            }

            // Create credit note items and calculate totals
            var creditNoteItems = new List<CreditNoteItem>();

            foreach (var itemDto in request.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId, cancellationToken);
                if (product == null || product.TenantId != tenantId)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    _logger.LogWarning("Product {ProductId} not found for tenant {TenantId}", itemDto.ProductId, tenantId);
                    return Result<CreditNoteDto>.Failure("Product not found");
                }

                var taxRate = await _unitOfWork.TaxRates.GetByIdAsync(itemDto.TaxRateId, cancellationToken);
                if (taxRate == null || taxRate.TenantId != tenantId)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    _logger.LogWarning("Tax rate {TaxRateId} not found for tenant {TenantId}", itemDto.TaxRateId, tenantId);
                    return Result<CreditNoteDto>.Failure("Tax rate not found");
                }

                var (subtotal, tax, total) = _taxCalculationService.CalculateLineItemTotals(
                    itemDto.Quantity,
                    product.UnitPrice,
                    taxRate.Rate);

                creditNoteItems.Add(new CreditNoteItem
                {
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
                });
            }

            // Calculate totals
            var itemDtosForCalc = creditNoteItems.Select(ii => new InvoiceItemDto
            {
                Quantity = ii.Quantity,
                UnitPrice = ii.UnitPrice,
                TaxRate = ii.TaxRate
            }).ToList();

            var (creditNoteSubtotal, creditNoteTax, creditNoteTotal) = _taxCalculationService.CalculateInvoiceTotals(itemDtosForCalc);

            var issueDate = request.IssueDate.HasValue
                ? DateTime.SpecifyKind(request.IssueDate.Value, DateTimeKind.Utc)
                : DateTime.UtcNow;

            var creditNote = new CreditNote
            {
                TenantId = tenantId,
                CreditNoteNumber = creditNoteNumber,
                CustomerId = request.CustomerId,
                OriginalInvoiceId = request.OriginalInvoiceId,
                OriginalInvoiceNumber = originalInvoice.InvoiceNumber,
                OriginalInvoiceDate = originalInvoice.IssueDate,
                EmissionPointId = request.EmissionPointId,
                IssueDate = issueDate,
                Status = InvoiceStatus.Draft,
                SubtotalAmount = creditNoteSubtotal,
                TaxAmount = creditNoteTax,
                TotalAmount = creditNoteTotal,
                ValueModification = creditNoteTotal,
                Reason = request.Reason,
                Notes = request.Notes,
                IsPhysicalReturn = request.IsPhysicalReturn,
                PaymentMethod = originalInvoice.PaymentMethod,
                Environment = config.Environment
            };

            await _unitOfWork.CreditNotes.AddAsync(creditNote, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var item in creditNoteItems)
            {
                item.CreditNoteId = creditNote.Id;
                await _unitOfWork.CreditNoteItems.AddAsync(item, cancellationToken);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Create inventory return movements when this is a physical return
            if (request.IsPhysicalReturn)
            {
                if (!originalInvoice.WarehouseId.HasValue)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    _logger.LogWarning("Cannot create stock return: original invoice {InvoiceId} has no warehouse assigned", originalInvoice.Id);
                    return Result<CreditNoteDto>.Failure("Cannot create stock return: the original invoice has no warehouse assigned");
                }

                foreach (var item in creditNoteItems)
                {
                    var movement = new StockMovement
                    {
                        TenantId = tenantId,
                        MovementType = MovementType.Return,
                        ProductId = item.ProductId,
                        WarehouseId = originalInvoice.WarehouseId.Value,
                        Quantity = item.Quantity,
                        UnitCost = item.UnitPrice,
                        TotalCost = item.SubtotalAmount,
                        Reference = creditNote.CreditNoteNumber,
                        Notes = $"Credit Note {creditNote.CreditNoteNumber}",
                        MovementDate = creditNote.IssueDate
                    };
                    await _unitOfWork.StockMovements.AddAsync(movement, cancellationToken);
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("{Count} stock return movement(s) created for credit note {CreditNoteNumber}", creditNoteItems.Count, creditNote.CreditNoteNumber);
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Credit note {CreditNoteNumber} created successfully for tenant {TenantId}", creditNoteNumber, tenantId);

            var dto = new CreditNoteDto
            {
                Id = creditNote.Id,
                TenantId = creditNote.TenantId,
                CreditNoteNumber = creditNote.CreditNoteNumber,
                CustomerId = creditNote.CustomerId,
                CustomerName = customer.Name,
                EmissionPointId = creditNote.EmissionPointId,
                EmissionPointCode = emissionPoint.EmissionPointCode,
                EmissionPointName = emissionPoint.Name,
                EstablishmentCode = establishment.EstablishmentCode,
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
                IsPhysicalReturn = creditNote.IsPhysicalReturn,
                PaymentMethod = creditNote.PaymentMethod,
                Environment = creditNote.Environment,
                DocumentType = creditNote.DocumentType,
                CreatedAt = creditNote.CreatedAt,
                UpdatedAt = creditNote.UpdatedAt,
                IsEditable = creditNote.IsEditable,
                Items = creditNoteItems.Select(ii => new CreditNoteItemDto
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
            _logger.LogError(ex, "Error creating credit note");
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<CreditNoteDto>.Failure("An error occurred while creating the credit note");
        }
    }
}
