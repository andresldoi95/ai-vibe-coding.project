using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Invoices.Queries.GetInvoiceById;

public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetInvoiceByIdQueryHandler> _logger;

    public GetInvoiceByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetInvoiceByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            var invoice = await _unitOfWork.Invoices.GetWithItemsAsync(request.Id, tenantId, cancellationToken);
            if (invoice == null)
            {
                return Result<InvoiceDto>.Failure("Invoice not found");
            }

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
                IsEditable = invoice.Status == Domain.Enums.InvoiceStatus.Draft && !invoice.IsDeleted
            };

            return Result<InvoiceDto>.Success(invoiceDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoice {InvoiceId}", request.Id);
            return Result<InvoiceDto>.Failure("An error occurred while retrieving the invoice");
        }
    }
}
