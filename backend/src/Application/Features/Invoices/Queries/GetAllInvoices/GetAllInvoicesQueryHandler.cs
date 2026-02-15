using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.Invoices.Queries.GetAllInvoices;

public class GetAllInvoicesQueryHandler : IRequestHandler<GetAllInvoicesQuery, Result<List<InvoiceDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetAllInvoicesQueryHandler> _logger;

    public GetAllInvoicesQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetAllInvoicesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<List<InvoiceDto>>> Handle(GetAllInvoicesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            // Get all invoices (base query)
            var allInvoices = await _unitOfWork.Invoices.GetAllAsync(cancellationToken);
            var invoices = allInvoices.Where(i => i.TenantId == tenantId && !i.IsDeleted).ToList();

            // Apply filters
            if (request.CustomerId.HasValue)
            {
                invoices = invoices.Where(i => i.CustomerId == request.CustomerId.Value).ToList();
            }

            if (request.Status.HasValue)
            {
                invoices = invoices.Where(i => i.Status == request.Status.Value).ToList();
            }

            if (request.DateFrom.HasValue)
            {
                invoices = invoices.Where(i => i.IssueDate >= request.DateFrom.Value).ToList();
            }

            if (request.DateTo.HasValue)
            {
                invoices = invoices.Where(i => i.IssueDate <= request.DateTo.Value).ToList();
            }

            // Get customers for names
            var customerIds = invoices.Select(i => i.CustomerId).Distinct().ToList();
            var customers = new Dictionary<Guid, string>();
            foreach (var customerId in customerIds)
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(customerId, cancellationToken);
                if (customer != null)
                {
                    customers[customerId] = customer.Name;
                }
            }

            // Get emission points for codes and names
            var emissionPointIds = invoices
                .Where(i => i.EmissionPointId.HasValue)
                .Select(i => i.EmissionPointId!.Value)
                .Distinct()
                .ToList();

            var emissionPointData = new Dictionary<Guid, (string Code, string Name, string EstablishmentCode)>();
            foreach (var emissionPointId in emissionPointIds)
            {
                var emissionPoint = await _unitOfWork.EmissionPoints.GetByIdAsync(emissionPointId, cancellationToken);

                if (emissionPoint != null && emissionPoint.Establishment != null)
                {
                    emissionPointData[emissionPointId] = (
                        emissionPoint.EmissionPointCode,
                        emissionPoint.Name,
                        emissionPoint.Establishment.EstablishmentCode
                    );
                }
            }

            var invoiceDtos = invoices.Select(i => new InvoiceDto
            {
                Id = i.Id,
                TenantId = i.TenantId,
                InvoiceNumber = i.InvoiceNumber,
                CustomerId = i.CustomerId,
                CustomerName = customers.ContainsKey(i.CustomerId) ? customers[i.CustomerId] : string.Empty,
                EmissionPointId = i.EmissionPointId,
                EmissionPointCode = i.EmissionPointId.HasValue && emissionPointData.ContainsKey(i.EmissionPointId.Value)
                    ? emissionPointData[i.EmissionPointId.Value].Code
                    : null,
                EmissionPointName = i.EmissionPointId.HasValue && emissionPointData.ContainsKey(i.EmissionPointId.Value)
                    ? emissionPointData[i.EmissionPointId.Value].Name
                    : null,
                EstablishmentCode = i.EmissionPointId.HasValue && emissionPointData.ContainsKey(i.EmissionPointId.Value)
                    ? emissionPointData[i.EmissionPointId.Value].EstablishmentCode
                    : null,
                IssueDate = i.IssueDate,
                DueDate = i.DueDate,
                Status = i.Status,
                SubtotalAmount = i.SubtotalAmount,
                TaxAmount = i.TaxAmount,
                TotalAmount = i.TotalAmount,
                Notes = i.Notes,
                WarehouseId = i.WarehouseId,
                SriAuthorization = i.SriAuthorization,
                AuthorizationDate = i.AuthorizationDate,
                PaidDate = i.PaidDate,
                Items = new List<InvoiceItemDto>(), // Empty for list view
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt,
                IsEditable = i.Status == InvoiceStatus.Draft && !i.IsDeleted
            })
            .OrderByDescending(i => i.IssueDate)
            .ToList();

            return Result<List<InvoiceDto>>.Success(invoiceDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoices");
            return Result<List<InvoiceDto>>.Failure("An error occurred while retrieving invoices");
        }
    }
}
