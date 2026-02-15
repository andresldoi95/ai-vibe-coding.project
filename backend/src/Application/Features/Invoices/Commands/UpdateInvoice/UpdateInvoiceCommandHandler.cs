using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.Invoices.Commands.UpdateInvoice;

public class UpdateInvoiceCommandHandler : IRequestHandler<UpdateInvoiceCommand, Result<InvoiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ITaxCalculationService _taxCalculationService;
    private readonly ILogger<UpdateInvoiceCommandHandler> _logger;

    public UpdateInvoiceCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ITaxCalculationService taxCalculationService,
        ILogger<UpdateInvoiceCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _taxCalculationService = taxCalculationService;
        _logger = logger;
    }

    public async Task<Result<InvoiceDto>> Handle(UpdateInvoiceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            // Start transaction
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // Get existing invoice with items
            var invoice = await _unitOfWork.Invoices.GetWithItemsAsync(request.Id, tenantId, cancellationToken);
            if (invoice == null || invoice.TenantId != tenantId)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<InvoiceDto>.Failure("Invoice not found");
            }

            // Check if invoice is editable
            if (!invoice.IsEditable)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<InvoiceDto>.Failure("Invoice cannot be edited. Only draft invoices can be modified.");
            }

            // Validate customer exists
            var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer == null || customer.TenantId != tenantId)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<InvoiceDto>.Failure("Customer not found");
            }

            // Update basic invoice fields
            invoice.CustomerId = request.CustomerId;
            invoice.WarehouseId = request.WarehouseId;
            invoice.Notes = request.Notes;

            if (request.IssueDate.HasValue)
            {
                invoice.IssueDate = DateTime.SpecifyKind(request.IssueDate.Value, DateTimeKind.Utc);
            }

            // If warehouse changed and invoice had stock movements, reverse them
            if (invoice.WarehouseId.HasValue)
            {
                var existingMovements = await _unitOfWork.StockMovements
                    .GetByReferenceAsync(invoice.InvoiceNumber, cancellationToken);

                foreach (var movement in existingMovements)
                {
                    // Delete old stock movement
                    await _unitOfWork.StockMovements.DeleteAsync(movement, cancellationToken);
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // Track existing item IDs
            var existingItemIds = invoice.Items.Select(i => i.Id).ToHashSet();
            var updatedItemIds = request.Items.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

            // Delete items that are no longer in the request
            var itemsToDelete = invoice.Items.Where(i => !updatedItemIds.Contains(i.Id)).ToList();
            foreach (var item in itemsToDelete)
            {
                await _unitOfWork.InvoiceItems.DeleteAsync(item, cancellationToken);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Process updated and new items
            var newInvoiceItems = new List<InvoiceItem>();

            foreach (var itemDto in request.Items)
            {
                // Get product
                var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId, cancellationToken);
                if (product == null || product.TenantId != tenantId)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<InvoiceDto>.Failure($"Product not found");
                }

                // Get tax rate
                var taxRate = await _unitOfWork.TaxRates.GetByIdAsync(itemDto.TaxRateId, cancellationToken);
                if (taxRate == null || taxRate.TenantId != tenantId)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<InvoiceDto>.Failure($"Tax rate not found");
                }

                // Calculate line totals
                var (subtotal, tax, total) = _taxCalculationService.CalculateLineItemTotals(
                    itemDto.Quantity,
                    product.UnitPrice,
                    taxRate.Rate);

                if (itemDto.Id.HasValue && existingItemIds.Contains(itemDto.Id.Value))
                {
                    // Update existing item
                    var existingItem = invoice.Items.First(i => i.Id == itemDto.Id.Value);
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

                    await _unitOfWork.InvoiceItems.UpdateAsync(existingItem);
                    newInvoiceItems.Add(existingItem);
                }
                else
                {
                    // Add new item
                    var newItem = new InvoiceItem
                    {
                        InvoiceId = invoice.Id,
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

                    await _unitOfWork.InvoiceItems.AddAsync(newItem, cancellationToken);
                    newInvoiceItems.Add(newItem);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Recalculate invoice totals
            var itemDtosForCalculation = newInvoiceItems.Select(ii => new InvoiceItemDto
            {
                Quantity = ii.Quantity,
                UnitPrice = ii.UnitPrice,
                TaxRate = ii.TaxRate
            }).ToList();

            var (invoiceSubtotal, invoiceTax, invoiceTotal) = _taxCalculationService.CalculateInvoiceTotals(itemDtosForCalculation);

            invoice.SubtotalAmount = invoiceSubtotal;
            invoice.TaxAmount = invoiceTax;
            invoice.TotalAmount = invoiceTotal;

            await _unitOfWork.Invoices.UpdateAsync(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Create new stock movements if warehouse is set
            if (invoice.WarehouseId.HasValue)
            {
                foreach (var item in newInvoiceItems)
                {
                    var stockMovement = new StockMovement
                    {
                        TenantId = tenantId,
                        MovementType = MovementType.Sale,
                        ProductId = item.ProductId,
                        WarehouseId = invoice.WarehouseId.Value,
                        Quantity = -item.Quantity, // Negative for sale
                        UnitCost = item.UnitPrice,
                        TotalCost = item.SubtotalAmount,
                        Reference = invoice.InvoiceNumber,
                        Notes = $"Invoice {invoice.InvoiceNumber} (Updated)",
                        MovementDate = invoice.IssueDate
                    };

                    await _unitOfWork.StockMovements.AddAsync(stockMovement, cancellationToken);
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Invoice {InvoiceNumber} updated successfully for tenant {TenantId}", invoice.InvoiceNumber, tenantId);

            // Reload invoice with complete data for response
            var updatedInvoice = await _unitOfWork.Invoices.GetWithItemsAsync(invoice.Id, tenantId, cancellationToken);
            var emissionPoint = await _unitOfWork.EmissionPoints.GetByIdAsync(updatedInvoice!.EmissionPointId!.Value, cancellationToken);

            // Build response DTO
            var invoiceDto = new InvoiceDto
            {
                Id = updatedInvoice.Id,
                TenantId = updatedInvoice.TenantId,
                InvoiceNumber = updatedInvoice.InvoiceNumber,
                CustomerId = updatedInvoice.CustomerId,
                CustomerName = customer.Name,
                EmissionPointId = updatedInvoice.EmissionPointId,
                EmissionPointCode = emissionPoint?.EmissionPointCode,
                EmissionPointName = emissionPoint?.Name,
                EstablishmentCode = emissionPoint?.Establishment?.EstablishmentCode,
                IssueDate = updatedInvoice.IssueDate,
                DueDate = updatedInvoice.DueDate,
                Status = updatedInvoice.Status,
                SubtotalAmount = updatedInvoice.SubtotalAmount,
                TaxAmount = updatedInvoice.TaxAmount,
                TotalAmount = updatedInvoice.TotalAmount,
                Notes = updatedInvoice.Notes,
                WarehouseId = updatedInvoice.WarehouseId,
                SriAuthorization = updatedInvoice.SriAuthorization,
                AuthorizationDate = updatedInvoice.AuthorizationDate,
                PaidDate = updatedInvoice.PaidDate,
                Items = newInvoiceItems.Select(ii => new InvoiceItemDto
                {
                    Id = ii.Id,
                    InvoiceId = ii.InvoiceId,
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
                }).ToList(),
                CreatedAt = updatedInvoice.CreatedAt,
                UpdatedAt = updatedInvoice.UpdatedAt,
                IsEditable = updatedInvoice.IsEditable
            };

            return Result<InvoiceDto>.Success(invoiceDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating invoice");
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<InvoiceDto>.Failure("An error occurred while updating the invoice");
        }
    }
}
