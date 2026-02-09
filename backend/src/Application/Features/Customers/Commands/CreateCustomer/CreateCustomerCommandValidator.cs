using FluentValidation;

namespace SaaS.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Customer name is required")
            .MaximumLength(256).WithMessage("Customer name cannot exceed 256 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters");

        RuleFor(x => x.Phone)
            .MaximumLength(50).WithMessage("Phone cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.TaxId)
            .MaximumLength(50).WithMessage("Tax ID cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.TaxId));

        RuleFor(x => x.ContactPerson)
            .MaximumLength(256).WithMessage("Contact person cannot exceed 256 characters")
            .When(x => !string.IsNullOrEmpty(x.ContactPerson));

        // Billing Address Validations
        RuleFor(x => x.BillingStreet)
            .MaximumLength(512).WithMessage("Billing street cannot exceed 512 characters")
            .When(x => !string.IsNullOrEmpty(x.BillingStreet));

        RuleFor(x => x.BillingCity)
            .MaximumLength(100).WithMessage("Billing city cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.BillingCity));

        RuleFor(x => x.BillingState)
            .MaximumLength(100).WithMessage("Billing state cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.BillingState));

        RuleFor(x => x.BillingPostalCode)
            .MaximumLength(20).WithMessage("Billing postal code cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.BillingPostalCode));

        RuleFor(x => x.BillingCountry)
            .MaximumLength(100).WithMessage("Billing country cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.BillingCountry));

        // Shipping Address Validations
        RuleFor(x => x.ShippingStreet)
            .MaximumLength(512).WithMessage("Shipping street cannot exceed 512 characters")
            .When(x => !string.IsNullOrEmpty(x.ShippingStreet));

        RuleFor(x => x.ShippingCity)
            .MaximumLength(100).WithMessage("Shipping city cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ShippingCity));

        RuleFor(x => x.ShippingState)
            .MaximumLength(100).WithMessage("Shipping state cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ShippingState));

        RuleFor(x => x.ShippingPostalCode)
            .MaximumLength(20).WithMessage("Shipping postal code cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.ShippingPostalCode));

        RuleFor(x => x.ShippingCountry)
            .MaximumLength(100).WithMessage("Shipping country cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ShippingCountry));

        // Additional Information
        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.Website)
            .MaximumLength(256).WithMessage("Website cannot exceed 256 characters")
            .When(x => !string.IsNullOrEmpty(x.Website));
    }
}
