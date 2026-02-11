using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.Invoices.Commands.CreateInvoice;

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, Result<InvoiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IInvoiceNumberService _invoiceNumberService;
    private readonly ITaxCalculationService _taxCalculationService;
    private readonly ILogger<CreateInvoiceCommandHandler> _logger;

    public CreateInvoiceCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        IInvoiceNumberService invoiceNumberService,
        ITaxCalculationService taxCalculationService,
        ILogger<CreateInvoiceCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _invoiceNumberService = invoiceNumberService;
        _taxCalculationService = taxCalculationService;
        _logger = logger;
    }

    public async Task<Result<InvoiceDto>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            // Start transaction
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // Validate customer exists
            var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer == null || customer.TenantId != tenantId)
            {
                return Result<InvoiceDto>.Failure("Customer not found");
            }

            // Get configuration
            var config = await _unitOfWork.InvoiceConfigurations.GetByTenantAsync(tenantId, cancellationToken);
            if (config == null)
            {
                return Result<InvoiceDto>.Failure("Invoice configuration not found");
            }

            // Generate invoice number
            var invoiceNumber = await _invoiceNumberService.GenerateNextInvoiceNumberAsync(tenantId, cancellationToken);

            // Create invoice items and calculate totals
            var invoiceItems = new List<InvoiceItem>();
            var itemDtos = new List<InvoiceItemDto>();

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

                var invoiceItem = new InvoiceItem
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
                };

                invoiceItems.Add(invoiceItem);
            }

            // Calculate invoice totals
            var itemDtosForCalculation = invoiceItems.Select(ii => new InvoiceItemDto
            {
                Quantity = ii.Quantity,
                UnitPrice = ii.UnitPrice,
                TaxRate = ii.TaxRate
            }).ToList();

            var (invoiceSubtotal, invoiceTax, invoiceTotal) = _taxCalculationService.CalculateInvoiceTotals(itemDtosForCalculation);

            // Ensure dates are UTC
            var issueDate = request.IssueDate.HasValue
                ? DateTime.SpecifyKind(request.IssueDate.Value, DateTimeKind.Utc)
                : DateTime.UtcNow;

            var dueDate = request.DueDate.HasValue
                ? DateTime.SpecifyKind(request.DueDate.Value, DateTimeKind.Utc)
                : DateTime.UtcNow.AddDays(config.DueDays);

            // Create invoice
            var invoice = new Invoice
            {
                TenantId = tenantId,
                InvoiceNumber = invoiceNumber,
                CustomerId = request.CustomerId,
                IssueDate = issueDate,
                DueDate = dueDate,
                Status = InvoiceStatus.Draft,
                SubtotalAmount = invoiceSubtotal,
                TaxAmount = invoiceTax,
                TotalAmount = invoiceTotal,
                Notes = request.Notes,
                WarehouseId = request.WarehouseId ?? config.DefaultWarehouseId,
                SriAuthorization = request.SriAuthorization
            };

            await _unitOfWork.Invoices.AddAsync(invoice, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Add items to invoice
            foreach (var item in invoiceItems)
            {
                item.InvoiceId = invoice.Id;
                await _unitOfWork.InvoiceItems.AddAsync(item, cancellationToken);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Create stock movements (reduce inventory)
            var warehouseId = invoice.WarehouseId;
            if (warehouseId.HasValue)
            {
                foreach (var item in invoiceItems)
                {
                    var stockMovement = new StockMovement
                    {
                        TenantId = tenantId,
                        MovementType = MovementType.Sale,
                        ProductId = item.ProductId,
                        WarehouseId = warehouseId.Value,
                        Quantity = -item.Quantity, // Negative for sale
                        UnitCost = item.UnitPrice,
                        TotalCost = item.SubtotalAmount,
                        Reference = invoice.InvoiceNumber,
                        Notes = $"Invoice {invoice.InvoiceNumber}",
                        MovementDate = invoice.IssueDate
                    };

                    await _unitOfWork.StockMovements.AddAsync(stockMovement, cancellationToken);
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Invoice {InvoiceNumber} created successfully for tenant {TenantId}", invoiceNumber, tenantId);

            // Build response DTO
            var invoiceDto = new InvoiceDto
            {
                Id = invoice.Id,
                TenantId = invoice.TenantId,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerId = invoice.CustomerId,
                CustomerName = customer.Name,
                IssueDate = invoice.IssueDate,
                DueDate = invoice.DueDate,
                Status = invoice.Status,
                SubtotalAmount = invoice.SubtotalAmount,
                TaxAmount = invoice.TaxAmount,
                TotalAmount = invoice.TotalAmount,
                Notes = invoice.Notes,
                WarehouseId = invoice.WarehouseId,
                SriAuthorization = invoice.SriAuthorization,
                AuthorizationDate = invoice.AuthorizationDate,
                PaidDate = invoice.PaidDate,
                Items = invoiceItems.Select(ii => new InvoiceItemDto
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
                CreatedAt = invoice.CreatedAt,
                UpdatedAt = invoice.UpdatedAt,
                IsEditable = invoice.IsEditable
            };

            return Result<InvoiceDto>.Success(invoiceDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice");
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<InvoiceDto>.Failure("An error occurred while creating the invoice");
        }
    }
}
