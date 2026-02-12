using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Customers.Queries.GetAllCustomers;

/// <summary>
/// Handler for getting all customers with optional filters
/// </summary>
public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, Result<List<CustomerDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetAllCustomersQueryHandler> _logger;

    public GetAllCustomersQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetAllCustomersQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<List<CustomerDto>>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<List<CustomerDto>>.Failure("Tenant context is required");
            }

            var customers = await _unitOfWork.Customers.GetAllByTenantAsync(
                _tenantContext.TenantId.Value,
                request.Filters,
                cancellationToken);

            var customerDtos = customers.Select(c => new CustomerDto
            {
                Id = c.Id,
                TenantId = c.TenantId,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                TaxId = c.TaxId,
                ContactPerson = c.ContactPerson,
                BillingStreet = c.BillingStreet,
                BillingCity = c.BillingCity,
                BillingState = c.BillingState,
                BillingPostalCode = c.BillingPostalCode,
                BillingCountryId = c.BillingCountryId,
                BillingCountryName = c.BillingCountry != null ? c.BillingCountry.Name : null,
                ShippingStreet = c.ShippingStreet,
                ShippingCity = c.ShippingCity,
                ShippingState = c.ShippingState,
                ShippingPostalCode = c.ShippingPostalCode,
                ShippingCountryId = c.ShippingCountryId,
                ShippingCountryName = c.ShippingCountry != null ? c.ShippingCountry.Name : null,
                Notes = c.Notes,
                Website = c.Website,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList();

            _logger.LogInformation(
                "Retrieved {Count} customers for tenant {TenantId}",
                customerDtos.Count,
                _tenantContext.TenantId.Value);

            return Result<List<CustomerDto>>.Success(customerDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customers");
            return Result<List<CustomerDto>>.Failure("An error occurred while retrieving customers");
        }
    }
}
