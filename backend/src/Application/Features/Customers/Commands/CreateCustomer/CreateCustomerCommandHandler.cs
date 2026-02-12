using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;
using SaaS.Domain.Entities;

namespace SaaS.Application.Features.Customers.Commands.CreateCustomer;

/// <summary>
/// Handler for creating a new customer
/// </summary>
public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<CreateCustomerCommandHandler> _logger;

    public CreateCustomerCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<CreateCustomerCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<CustomerDto>.Failure("Tenant context is required");
            }

            // Check if customer email already exists for this tenant
            var existingCustomerByEmail = await _unitOfWork.Customers.GetByEmailAsync(
                request.Email,
                _tenantContext.TenantId.Value,
                cancellationToken);

            if (existingCustomerByEmail != null)
            {
                return Result<CustomerDto>.Failure($"Customer with email '{request.Email}' already exists");
            }

            // Check if TaxId already exists for this tenant (if provided)
            if (!string.IsNullOrWhiteSpace(request.TaxId))
            {
                var existingCustomerByTaxId = await _unitOfWork.Customers.GetByTaxIdAsync(
                    request.TaxId,
                    _tenantContext.TenantId.Value,
                    cancellationToken);

                if (existingCustomerByTaxId != null)
                {
                    return Result<CustomerDto>.Failure($"Customer with Tax ID '{request.TaxId}' already exists");
                }
            }

            // Create customer entity
            var customer = new Customer
            {
                TenantId = _tenantContext.TenantId.Value,
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                TaxId = request.TaxId,
                ContactPerson = request.ContactPerson,
                BillingStreet = request.BillingStreet,
                BillingCity = request.BillingCity,
                BillingState = request.BillingState,
                BillingPostalCode = request.BillingPostalCode,
                BillingCountryId = request.BillingCountryId,
                ShippingStreet = request.ShippingStreet,
                ShippingCity = request.ShippingCity,
                ShippingState = request.ShippingState,
                ShippingPostalCode = request.ShippingPostalCode,
                ShippingCountryId = request.ShippingCountryId,
                Notes = request.Notes,
                Website = request.Website,
                IsActive = request.IsActive
            };

            await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Customer {Email} created successfully for tenant {TenantId}",
                customer.Email,
                customer.TenantId);

            // Map to DTO
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
                BillingCountryId = customer.BillingCountryId,
                BillingCountryName = null,
                ShippingStreet = customer.ShippingStreet,
                ShippingCity = customer.ShippingCity,
                ShippingState = customer.ShippingState,
                ShippingPostalCode = customer.ShippingPostalCode,
                ShippingCountryId = customer.ShippingCountryId,
                ShippingCountryName = null,
                Notes = customer.Notes,
                Website = customer.Website,
                IsActive = customer.IsActive,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt
            };

            return Result<CustomerDto>.Success(customerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer");
            return Result<CustomerDto>.Failure("An error occurred while creating the customer");
        }
    }
}
