using FluentValidation;

namespace SaaS.Application.Features.CreditNotes.Commands.CreateCreditNote;

public class CreateCreditNoteCommandValidator : AbstractValidator<CreateCreditNoteCommand>
{
    public CreateCreditNoteCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer is required");

        RuleFor(x => x.OriginalInvoiceId)
            .NotEmpty().WithMessage("Original invoice is required");

        RuleFor(x => x.EmissionPointId)
            .NotEmpty().WithMessage("Emission point is required");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason (motivo) is required")
            .MaximumLength(300).WithMessage("Reason must not exceed 300 characters");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Credit note must have at least one item");

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

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters");
    }
}
