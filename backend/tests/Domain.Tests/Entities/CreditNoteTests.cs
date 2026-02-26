using Xunit;
using FluentAssertions;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace Domain.Tests.Entities;

public class CreditNoteTests
{
    [Fact]
    public void CreditNote_ShouldInitialize_WithDefaultValues()
    {
        // Arrange & Act
        var creditNote = new CreditNote();

        // Assert
        creditNote.CreditNoteNumber.Should().BeEmpty();
        creditNote.CustomerId.Should().BeEmpty();
        creditNote.IssueDate.Should().Be(default(DateTime));
        creditNote.Status.Should().Be(InvoiceStatus.Draft);
        creditNote.SubtotalAmount.Should().Be(0);
        creditNote.TaxAmount.Should().Be(0);
        creditNote.TotalAmount.Should().Be(0);
        creditNote.Notes.Should().BeNull();
        creditNote.OriginalInvoiceId.Should().BeEmpty();
        creditNote.OriginalInvoiceNumber.Should().BeEmpty();
        creditNote.OriginalInvoiceDate.Should().Be(default(DateTime));
        creditNote.Reason.Should().BeEmpty();
        creditNote.ValueModification.Should().Be(0);
        creditNote.IsPhysicalReturn.Should().BeFalse();
        creditNote.EmissionPointId.Should().BeNull();
        creditNote.DocumentType.Should().Be(DocumentType.CreditNote);
        creditNote.AccessKey.Should().BeNull();
        creditNote.PaymentMethod.Should().Be(SriPaymentMethod.Cash);
        creditNote.XmlFilePath.Should().BeNull();
        creditNote.SignedXmlFilePath.Should().BeNull();
        creditNote.RideFilePath.Should().BeNull();
        creditNote.Environment.Should().Be(SriEnvironment.Test);
        creditNote.SriAuthorization.Should().BeNull();
        creditNote.AuthorizationDate.Should().BeNull();
    }

    [Fact]
    public void CreditNote_ShouldSet_AllRequiredProperties()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var originalInvoiceId = Guid.NewGuid();
        var emissionPointId = Guid.NewGuid();
        var issueDate = DateTime.UtcNow;
        var originalInvoiceDate = DateTime.UtcNow.AddDays(-5);
        var authorizationDate = DateTime.UtcNow.AddHours(1);

        // Act
        var creditNote = new CreditNote
        {
            TenantId = tenantId,
            CreditNoteNumber = "001-001-000000001",
            CustomerId = customerId,
            IssueDate = issueDate,
            Status = InvoiceStatus.Authorized,
            SubtotalAmount = 100.00m,
            TaxAmount = 15.00m,
            TotalAmount = 115.00m,
            Notes = "Credit note for defective product",
            OriginalInvoiceId = originalInvoiceId,
            OriginalInvoiceNumber = "001-001-000000010",
            OriginalInvoiceDate = originalInvoiceDate,
            Reason = "Producto defectuoso",
            ValueModification = 115.00m,
            IsPhysicalReturn = true,
            EmissionPointId = emissionPointId,
            DocumentType = DocumentType.CreditNote,
            AccessKey = "4902202504019990000000001100100100000000122345671",
            PaymentMethod = SriPaymentMethod.BankTransfer,
            XmlFilePath = "/xml/cn-001.xml",
            SignedXmlFilePath = "/xml/cn-001-signed.xml",
            RideFilePath = "/ride/cn-001.pdf",
            Environment = SriEnvironment.Production,
            SriAuthorization = "4902202504019990000000001100100100000000122345671",
            AuthorizationDate = authorizationDate
        };

        // Assert
        creditNote.TenantId.Should().Be(tenantId);
        creditNote.CreditNoteNumber.Should().Be("001-001-000000001");
        creditNote.CustomerId.Should().Be(customerId);
        creditNote.IssueDate.Should().Be(issueDate);
        creditNote.Status.Should().Be(InvoiceStatus.Authorized);
        creditNote.SubtotalAmount.Should().Be(100.00m);
        creditNote.TaxAmount.Should().Be(15.00m);
        creditNote.TotalAmount.Should().Be(115.00m);
        creditNote.Notes.Should().Be("Credit note for defective product");
        creditNote.OriginalInvoiceId.Should().Be(originalInvoiceId);
        creditNote.OriginalInvoiceNumber.Should().Be("001-001-000000010");
        creditNote.OriginalInvoiceDate.Should().Be(originalInvoiceDate);
        creditNote.Reason.Should().Be("Producto defectuoso");
        creditNote.ValueModification.Should().Be(115.00m);
        creditNote.IsPhysicalReturn.Should().BeTrue();
        creditNote.EmissionPointId.Should().Be(emissionPointId);
        creditNote.DocumentType.Should().Be(DocumentType.CreditNote);
        creditNote.AccessKey.Should().Be("4902202504019990000000001100100100000000122345671");
        creditNote.PaymentMethod.Should().Be(SriPaymentMethod.BankTransfer);
        creditNote.XmlFilePath.Should().Be("/xml/cn-001.xml");
        creditNote.SignedXmlFilePath.Should().Be("/xml/cn-001-signed.xml");
        creditNote.RideFilePath.Should().Be("/ride/cn-001.pdf");
        creditNote.Environment.Should().Be(SriEnvironment.Production);
        creditNote.SriAuthorization.Should().Be("4902202504019990000000001100100100000000122345671");
        creditNote.AuthorizationDate.Should().Be(authorizationDate);
    }

    [Fact]
    public void CreditNote_ShouldAllowNull_OptionalProperties()
    {
        // Arrange & Act
        var creditNote = new CreditNote
        {
            Notes = null,
            EmissionPointId = null,
            AccessKey = null,
            XmlFilePath = null,
            SignedXmlFilePath = null,
            RideFilePath = null,
            SriAuthorization = null,
            AuthorizationDate = null
        };

        // Assert
        creditNote.Notes.Should().BeNull();
        creditNote.EmissionPointId.Should().BeNull();
        creditNote.AccessKey.Should().BeNull();
        creditNote.XmlFilePath.Should().BeNull();
        creditNote.SignedXmlFilePath.Should().BeNull();
        creditNote.RideFilePath.Should().BeNull();
        creditNote.SriAuthorization.Should().BeNull();
        creditNote.AuthorizationDate.Should().BeNull();
    }

    [Fact]
    public void CreditNote_IsEditable_ReturnsTrue_WhenDraftAndNotDeleted()
    {
        // Arrange
        var creditNote = new CreditNote
        {
            Status = InvoiceStatus.Draft,
            IsDeleted = false
        };

        // Act & Assert
        creditNote.IsEditable.Should().BeTrue();
    }

    [Fact]
    public void CreditNote_IsEditable_ReturnsFalse_WhenDeleted()
    {
        // Arrange
        var creditNote = new CreditNote
        {
            Status = InvoiceStatus.Draft,
            IsDeleted = true
        };

        // Act & Assert
        creditNote.IsEditable.Should().BeFalse();
    }

    [Theory]
    [InlineData(InvoiceStatus.PendingSignature)]
    [InlineData(InvoiceStatus.PendingAuthorization)]
    [InlineData(InvoiceStatus.Authorized)]
    [InlineData(InvoiceStatus.Rejected)]
    [InlineData(InvoiceStatus.Cancelled)]
    public void CreditNote_IsEditable_ReturnsFalse_WhenNotDraft(InvoiceStatus status)
    {
        // Arrange
        var creditNote = new CreditNote
        {
            Status = status,
            IsDeleted = false
        };

        // Act & Assert
        creditNote.IsEditable.Should().BeFalse();
    }

    [Fact]
    public void CreditNote_IsEditable_Combinations_AreCorrect()
    {
        // All combinations
        var draftNotDeleted = new CreditNote { Status = InvoiceStatus.Draft, IsDeleted = false };
        var draftDeleted = new CreditNote { Status = InvoiceStatus.Draft, IsDeleted = true };
        var authorizedNotDeleted = new CreditNote { Status = InvoiceStatus.Authorized, IsDeleted = false };
        var rejectedNotDeleted = new CreditNote { Status = InvoiceStatus.Rejected, IsDeleted = false };

        // Assert
        draftNotDeleted.IsEditable.Should().BeTrue();
        draftDeleted.IsEditable.Should().BeFalse();
        authorizedNotDeleted.IsEditable.Should().BeFalse();
        rejectedNotDeleted.IsEditable.Should().BeFalse();
    }

    [Fact]
    public void CreditNote_Items_InitializesAsEmptyList()
    {
        // Arrange & Act
        var creditNote = new CreditNote();

        // Assert
        creditNote.Items.Should().NotBeNull();
        creditNote.Items.Should().BeEmpty();
    }

    [Fact]
    public void CreditNote_DocumentType_IsAlwaysCreditNote()
    {
        // Arrange & Act
        var creditNote = new CreditNote();

        // Assert
        creditNote.DocumentType.Should().Be(DocumentType.CreditNote);
    }

    [Theory]
    [InlineData(SriEnvironment.Test)]
    [InlineData(SriEnvironment.Production)]
    public void CreditNote_Environment_CanBeSet(SriEnvironment environment)
    {
        // Arrange & Act
        var creditNote = new CreditNote { Environment = environment };

        // Assert
        creditNote.Environment.Should().Be(environment);
    }

    [Theory]
    [InlineData(SriPaymentMethod.Cash)]
    [InlineData(SriPaymentMethod.BankTransfer)]
    [InlineData(SriPaymentMethod.CreditCard)]
    [InlineData(SriPaymentMethod.DebitCard)]
    public void CreditNote_PaymentMethod_CanBeSet(SriPaymentMethod paymentMethod)
    {
        // Arrange & Act
        var creditNote = new CreditNote { PaymentMethod = paymentMethod };

        // Assert
        creditNote.PaymentMethod.Should().Be(paymentMethod);
    }

    [Theory]
    [InlineData(100.00, 15.00, 115.00)]
    [InlineData(0.00, 0.00, 0.00)]
    [InlineData(999.99, 149.99, 1149.98)]
    public void CreditNote_Amounts_CanBeSetToValidDecimals(decimal subtotal, decimal tax, decimal total)
    {
        // Arrange & Act
        var creditNote = new CreditNote
        {
            SubtotalAmount = subtotal,
            TaxAmount = tax,
            TotalAmount = total
        };

        // Assert
        creditNote.SubtotalAmount.Should().Be(subtotal);
        creditNote.TaxAmount.Should().Be(tax);
        creditNote.TotalAmount.Should().Be(total);
    }

    [Fact]
    public void CreditNote_IsPhysicalReturn_DefaultsToFalse()
    {
        // Arrange & Act
        var creditNote = new CreditNote();

        // Assert
        creditNote.IsPhysicalReturn.Should().BeFalse();
    }

    [Fact]
    public void CreditNote_IsPhysicalReturn_CanBeSetToTrue()
    {
        // Arrange & Act
        var creditNote = new CreditNote { IsPhysicalReturn = true };

        // Assert
        creditNote.IsPhysicalReturn.Should().BeTrue();
    }
}
