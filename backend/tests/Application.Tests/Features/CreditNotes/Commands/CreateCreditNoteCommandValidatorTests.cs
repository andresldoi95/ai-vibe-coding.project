using FluentValidation.TestHelper;
using SaaS.Application.DTOs;
using SaaS.Application.Features.CreditNotes.Commands.CreateCreditNote;
using Xunit;

namespace Application.Tests.Features.CreditNotes.Commands;

public class CreateCreditNoteCommandValidatorTests
{
    private readonly CreateCreditNoteCommandValidator _validator = new();

    private static CreateCreditNoteCommand ValidCommand() => new()
    {
        CustomerId = Guid.NewGuid(),
        OriginalInvoiceId = Guid.NewGuid(),
        EmissionPointId = Guid.NewGuid(),
        Reason = "Devolución de mercadería por defecto",
        Items = new List<CreateCreditNoteItemDto>
        {
            new()
            {
                ProductId = Guid.NewGuid(),
                Quantity = 2,
                TaxRateId = Guid.NewGuid(),
                Description = "Producto A"
            }
        }
    };

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ── CustomerId ────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyCustomerId_ShouldFail()
    {
        var command = ValidCommand() with { CustomerId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CustomerId)
              .WithErrorMessage("Customer is required");
    }

    // ── OriginalInvoiceId ─────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyOriginalInvoiceId_ShouldFail()
    {
        var command = ValidCommand() with { OriginalInvoiceId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OriginalInvoiceId)
              .WithErrorMessage("Original invoice is required");
    }

    // ── EmissionPointId ───────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyEmissionPointId_ShouldFail()
    {
        var command = ValidCommand() with { EmissionPointId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.EmissionPointId)
              .WithErrorMessage("Emission point is required");
    }

    // ── Reason ────────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyReason_ShouldFail()
    {
        var command = ValidCommand() with { Reason = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Reason)
              .WithErrorMessage("Reason (motivo) is required");
    }

    [Fact]
    public void Validate_ReasonExceedsMaxLength_ShouldFail()
    {
        var command = ValidCommand() with { Reason = new string('A', 301) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Reason)
              .WithErrorMessage("Reason must not exceed 300 characters");
    }

    [Fact]
    public void Validate_ReasonAtMaxLength_ShouldPass()
    {
        var command = ValidCommand() with { Reason = new string('A', 300) };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }

    // ── Items ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyItems_ShouldFail()
    {
        var command = ValidCommand() with { Items = new List<CreateCreditNoteItemDto>() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Items)
              .WithErrorMessage("Credit note must have at least one item");
    }

    [Fact]
    public void Validate_ItemWithEmptyProductId_ShouldFail()
    {
        var command = ValidCommand() with
        {
            Items = new List<CreateCreditNoteItemDto>
            {
                new()
                {
                    ProductId = Guid.Empty,
                    Quantity = 1,
                    TaxRateId = Guid.NewGuid(),
                    Description = "Test"
                }
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Items[0].ProductId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_ItemWithInvalidQuantity_ShouldFail(int quantity)
    {
        var command = ValidCommand() with
        {
            Items = new List<CreateCreditNoteItemDto>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = quantity,
                    TaxRateId = Guid.NewGuid(),
                    Description = "Test"
                }
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Items[0].Quantity")
              .WithErrorMessage("Quantity must be greater than 0");
    }

    [Fact]
    public void Validate_ItemWithEmptyTaxRateId_ShouldFail()
    {
        var command = ValidCommand() with
        {
            Items = new List<CreateCreditNoteItemDto>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 1,
                    TaxRateId = Guid.Empty,
                    Description = "Test"
                }
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Items[0].TaxRateId");
    }

    [Fact]
    public void Validate_ItemWithEmptyDescription_ShouldFail()
    {
        var command = ValidCommand() with
        {
            Items = new List<CreateCreditNoteItemDto>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 1,
                    TaxRateId = Guid.NewGuid(),
                    Description = ""
                }
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Items[0].Description")
              .WithErrorMessage("Description is required");
    }

    [Fact]
    public void Validate_ItemWithDescriptionExceedingMaxLength_ShouldFail()
    {
        var command = ValidCommand() with
        {
            Items = new List<CreateCreditNoteItemDto>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 1,
                    TaxRateId = Guid.NewGuid(),
                    Description = new string('X', 1001)
                }
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Items[0].Description")
              .WithErrorMessage("Description must not exceed 1000 characters");
    }

    // ── Notes (optional) ─────────────────────────────────────────────────────

    [Fact]
    public void Validate_NullNotes_ShouldPass()
    {
        var command = ValidCommand() with { Notes = null };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public void Validate_NotesExceedingMaxLength_ShouldFail()
    {
        var command = ValidCommand() with { Notes = new string('N', 2001) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Notes)
              .WithErrorMessage("Notes must not exceed 2000 characters");
    }

    [Fact]
    public void Validate_NotesAtMaxLength_ShouldPass()
    {
        var command = ValidCommand() with { Notes = new string('N', 2000) };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Notes);
    }
}
