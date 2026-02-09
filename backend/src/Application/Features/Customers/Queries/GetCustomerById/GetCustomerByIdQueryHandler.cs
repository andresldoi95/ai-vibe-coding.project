using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Customers.Queries.GetCustomerById;

/// <summary>
/// Handler for getting a customer by ID
/// </summary>
public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetCustomerByIdQueryHandler> _logger;

    public GetCustomerByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetCustomerByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<CustomerDto>.Failure("Tenant context is required");
            }

            var customer = await _unitOfWork.Customers.GetByIdAsync(request.Id, cancellationToken);

            if (customer == null)
            {
                return Result<CustomerDto>.Failure("Customer not found");
            }

            // Verify customer belongs to the current tenant
            if (customer.TenantId != _tenantContext.TenantId.Value)
            {
                return Result<CustomerDto>.Failure("Unauthorized access to customer");
            }

            var customerDto = new CustomerDto
            {
                Id = customer.Id,
                TenantId = customer.TenantId,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                TaxId = customer.TaxId,
                ContactPerson = customer.ContactPerson,
                BillingStreet = customer.BillingStreet,
                BillingCity = customer.BillingCity,
                BillingState = customer.BillingState,
                BillingPostalCode = customer.BillingPostalCode,
                BillingCountry = customer.BillingCountry,
                ShippingStreet = customer.ShippingStreet,
                ShippingCity = customer.ShippingCity,
                ShippingState = customer.ShippingState,
                ShippingPostalCode = customer.ShippingPostalCode,
                ShippingCountry = customer.ShippingCountry,
                Notes = customer.Notes,
                Website = customer.Website,
                IsActive = customer.IsActive,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt
            };

            _logger.LogInformation(
                "Retrieved customer {Id} for tenant {TenantId}",
                customer.Id,
                customer.TenantId);

            return Result<CustomerDto>.Success(customerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer {Id}", request.Id);
            return Result<CustomerDto>.Failure("An error occurred while retrieving the customer");
        }
    }
}
