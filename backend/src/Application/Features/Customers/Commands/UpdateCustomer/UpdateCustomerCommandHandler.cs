using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Customers.Commands.UpdateCustomer;

/// <summary>
/// Handler for updating an existing customer
/// </summary>
public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result<CustomerDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<UpdateCustomerCommandHandler> _logger;

    public UpdateCustomerCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<UpdateCustomerCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<CustomerDto>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_tenantContext.TenantId.HasValue)
            {
                return Result<CustomerDto>.Failure("Tenant context is required");
            }

            // Get existing customer
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

            // Check if email is being changed and if new email already exists
            if (customer.Email != request.Email)
            {
                var existingCustomerByEmail = await _unitOfWork.Customers.GetByEmailAsync(
                    request.Email,
                    _tenantContext.TenantId.Value,
                    cancellationToken);

                if (existingCustomerByEmail != null)
                {
                    return Result<CustomerDto>.Failure($"Customer with email '{request.Email}' already exists");
                }
            }

            // Check if TaxId is being changed and if new TaxId already exists
            if (!string.IsNullOrWhiteSpace(request.TaxId) && customer.TaxId != request.TaxId)
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

            // Update customer properties
            customer.Name = request.Name;
            customer.Email = request.Email;
            customer.Phone = request.Phone;
            customer.TaxId = request.TaxId;
            customer.ContactPerson = request.ContactPerson;
            customer.BillingStreet = request.BillingStreet;
            customer.BillingCity = request.BillingCity;
            customer.BillingState = request.BillingState;
            customer.BillingPostalCode = request.BillingPostalCode;
            customer.BillingCountry = request.BillingCountry;
            customer.ShippingStreet = request.ShippingStreet;
            customer.ShippingCity = request.ShippingCity;
            customer.ShippingState = request.ShippingState;
            customer.ShippingPostalCode = request.ShippingPostalCode;
            customer.ShippingCountry = request.ShippingCountry;
            customer.Notes = request.Notes;
            customer.Website = request.Website;
            customer.IsActive = request.IsActive;

            await _unitOfWork.Customers.UpdateAsync(customer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Customer {Id} updated successfully for tenant {TenantId}",
                customer.Id,
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

            return Result<CustomerDto>.Success(customerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer {Id}", request.Id);
            return Result<CustomerDto>.Failure("An error occurred while updating the customer");
        }
    }
}
