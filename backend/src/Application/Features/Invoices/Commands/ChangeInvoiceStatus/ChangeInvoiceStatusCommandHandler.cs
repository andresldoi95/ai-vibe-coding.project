using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.Invoices.Commands.ChangeInvoiceStatus;

public class ChangeInvoiceStatusCommandHandler : IRequestHandler<ChangeInvoiceStatusCommand, Result<InvoiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<ChangeInvoiceStatusCommandHandler> _logger;

    public ChangeInvoiceStatusCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<ChangeInvoiceStatusCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<InvoiceDto>> Handle(ChangeInvoiceStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            var invoice = await _unitOfWork.Invoices.GetWithItemsAsync(request.Id, tenantId, cancellationToken);
            if (invoice == null)
            {
                return Result<InvoiceDto>.Failure("Invoice not found");
            }

            // Validate status transitions
            if (!IsValidStatusTransition(invoice.Status, request.NewStatus))
            {
                return Result<InvoiceDto>.Failure($"Cannot change status from {invoice.Status} to {request.NewStatus}");
            }

            invoice.Status = request.NewStatus;

            // Set paid date if status is Paid
            if (request.NewStatus == InvoiceStatus.Paid && !invoice.PaidDate.HasValue)
            {
                invoice.PaidDate = DateTime.UtcNow;
            }

            await _unitOfWork.Invoices.UpdateAsync(invoice, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Invoice {InvoiceId} status changed to {Status} for tenant {TenantId}",
                request.Id, request.NewStatus, tenantId);

            var invoiceDto = new InvoiceDto
            {
                Id = invoice.Id,
                TenantId = invoice.TenantId,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerId = invoice.CustomerId,
                CustomerName = invoice.Customer?.Name ?? string.Empty,
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
                Items = invoice.Items.Select(ii => new InvoiceItemDto
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
            _logger.LogError(ex, "Error changing invoice status for {InvoiceId}", request.Id);
            return Result<InvoiceDto>.Failure("An error occurred while changing the invoice status");
        }
    }

    private bool IsValidStatusTransition(InvoiceStatus currentStatus, InvoiceStatus newStatus)
    {
        // Draft can go to Sent or Cancelled
        if (currentStatus == InvoiceStatus.Draft)
            return newStatus == InvoiceStatus.Sent || newStatus == InvoiceStatus.Cancelled;

        // Sent can go to Paid, Overdue, or Cancelled
        if (currentStatus == InvoiceStatus.Sent)
            return newStatus == InvoiceStatus.Paid || newStatus == InvoiceStatus.Overdue || newStatus == InvoiceStatus.Cancelled;

        // Overdue can go to Paid or Cancelled
        if (currentStatus == InvoiceStatus.Overdue)
            return newStatus == InvoiceStatus.Paid || newStatus == InvoiceStatus.Cancelled;

        // Paid and Cancelled are final states
        return false;
    }
}
