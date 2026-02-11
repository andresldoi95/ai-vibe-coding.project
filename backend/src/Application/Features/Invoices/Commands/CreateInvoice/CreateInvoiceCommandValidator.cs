using FluentValidation;

namespace SaaS.Application.Features.Invoices.Commands.CreateInvoice;

public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer is required");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Invoice must have at least one item");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product is required");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");

            item.RuleFor(x => x.TaxRateId)
                .NotEmpty().WithMessage("Tax rate is required");

            item.RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");
        });

        RuleFor(x => x.SriAuthorization)
            .MaximumLength(49).WithMessage("SRI Authorization must not exceed 49 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters");
    }
}
