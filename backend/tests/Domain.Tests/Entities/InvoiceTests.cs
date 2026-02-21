using Xunit;
using FluentAssertions;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace Domain.Tests.Entities;

public class InvoiceTests
{
    [Fact]
    public void Invoice_ShouldInitialize_WithDefaultValues()
    {
        // Arrange & Act
        var invoice = new Invoice();

        // Assert
        invoice.InvoiceNumber.Should().BeEmpty();
        invoice.CustomerId.Should().BeEmpty();
        invoice.IssueDate.Should().Be(default(DateTime));
        invoice.DueDate.Should().Be(default(DateTime));
        invoice.Status.Should().Be(InvoiceStatus.Draft);
        invoice.SubtotalAmount.Should().Be(0);
        invoice.TaxAmount.Should().Be(0);
        invoice.TotalAmount.Should().Be(0);
        invoice.Notes.Should().BeNull();
        invoice.WarehouseId.Should().BeNull();
        invoice.EmissionPointId.Should().BeNull();
        invoice.DocumentType.Should().Be(DocumentType.Invoice);
        invoice.AccessKey.Should().BeNull();
        invoice.PaymentMethod.Should().Be(SriPaymentMethod.Cash);
        invoice.XmlFilePath.Should().BeNull();
        invoice.SignedXmlFilePath.Should().BeNull();
        invoice.Environment.Should().Be(SriEnvironment.Test);
        invoice.SriAuthorization.Should().BeNull();
        invoice.AuthorizationDate.Should().BeNull();
        invoice.PaidDate.Should().BeNull();
    }

    [Fact]
    public void Invoice_ShouldSet_AllRequiredProperties()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var emissionPointId = Guid.NewGuid();
        var issueDate = DateTime.UtcNow;
        var dueDate = DateTime.UtcNow.AddDays(30);

        // Act
        var invoice = new Invoice
        {
            TenantId = tenantId,
            InvoiceNumber = "INV-001",
            CustomerId = customerId,
            IssueDate = issueDate,
            DueDate = dueDate,
            Status = InvoiceStatus.Authorized,
            SubtotalAmount = 100.00m,
            TaxAmount = 12.00m,
            TotalAmount = 112.00m,
            Notes = "Test invoice",
            WarehouseId = warehouseId,
            EmissionPointId = emissionPointId,
            DocumentType = DocumentType.Invoice,
            AccessKey = "1234567890",
            PaymentMethod = SriPaymentMethod.CreditCard,
            XmlFilePath = "/path/to/xml",
            SignedXmlFilePath = "/path/to/signed",
            Environment = SriEnvironment.Production,
            SriAuthorization = "AUTH-123",
            AuthorizationDate = DateTime.UtcNow,
            PaidDate = DateTime.UtcNow
        };

        // Assert
        invoice.TenantId.Should().Be(tenantId);
        invoice.InvoiceNumber.Should().Be("INV-001");
        invoice.CustomerId.Should().Be(customerId);
        invoice.IssueDate.Should().Be(issueDate);
        invoice.DueDate.Should().Be(dueDate);
        invoice.Status.Should().Be(InvoiceStatus.Authorized);
        invoice.SubtotalAmount.Should().Be(100.00m);
        invoice.TaxAmount.Should().Be(12.00m);
        invoice.TotalAmount.Should().Be(112.00m);
        invoice.Notes.Should().Be("Test invoice");
        invoice.WarehouseId.Should().Be(warehouseId);
        invoice.EmissionPointId.Should().Be(emissionPointId);
        invoice.DocumentType.Should().Be(DocumentType.Invoice);
        invoice.AccessKey.Should().Be("1234567890");
        invoice.PaymentMethod.Should().Be(SriPaymentMethod.CreditCard);
        invoice.XmlFilePath.Should().Be("/path/to/xml");
        invoice.SignedXmlFilePath.Should().Be("/path/to/signed");
        invoice.Environment.Should().Be(SriEnvironment.Production);
        invoice.SriAuthorization.Should().Be("AUTH-123");
        invoice.AuthorizationDate.Should().NotBeNull();
        invoice.PaidDate.Should().NotBeNull();
    }

    [Fact]
    public void Invoice_IsEditable_ReturnsFalse_WhenNotDraft()
    {
        // Arrange
        var invoice = new Invoice { Status = InvoiceStatus.Authorized };

        // Act & Assert
        invoice.IsEditable.Should().BeFalse();
    }

    [Fact]
    public void Invoice_IsEditable_ReturnsFalse_WhenDeleted()
    {
        // Arrange
        var invoice = new Invoice
        {
            Status = InvoiceStatus.Draft,
            IsDeleted = true
        };

        // Act & Assert
        invoice.IsEditable.Should().BeFalse();
    }

    [Fact]
    public void Invoice_IsEditable_ReturnsTrue_WhenDraftAndNotDeleted()
    {
        // Arrange
        var invoice = new Invoice
        {
            Status = InvoiceStatus.Draft,
            IsDeleted = false
        };

        // Act & Assert
        invoice.IsEditable.Should().BeTrue();
    }

    [Theory]
    [InlineData(InvoiceStatus.Draft)]
    [InlineData(InvoiceStatus.Authorized)]
    [InlineData(InvoiceStatus.Paid)]
    [InlineData(InvoiceStatus.Cancelled)]
    [InlineData(InvoiceStatus.Overdue)]
    public void Invoice_Status_CanBeSet(InvoiceStatus status)
    {
        // Arrange & Act
        var invoice = new Invoice { Status = status };

        // Assert
        invoice.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(DocumentType.Invoice)]
    [InlineData(DocumentType.CreditNote)]
    [InlineData(DocumentType.DebitNote)]
    public void Invoice_DocumentType_CanBeSet(DocumentType documentType)
    {
        // Arrange & Act
        var invoice = new Invoice { DocumentType = documentType };

        // Assert
        invoice.DocumentType.Should().Be(documentType);
    }

    [Fact]
    public void Invoice_Items_InitializesAsEmptyList()
    {
        // Arrange & Act
        var invoice = new Invoice();

        // Assert
        invoice.Items.Should().NotBeNull();
        invoice.Items.Should().BeEmpty();
    }

    [Fact]
    public void Invoice_Payments_InitializesAsEmptyList()
    {
        // Arrange & Act
        var invoice = new Invoice();

        // Assert
        invoice.Payments.Should().NotBeNull();
        invoice.Payments.Should().BeEmpty();
    }

    [Fact]
    public void Invoice_IsEditable_Combinations_AreCorrect()
    {
        // Test all combinations of Status and IsDeleted
        var draftNotDeleted = new Invoice { Status = InvoiceStatus.Draft, IsDeleted = false };
        var draftDeleted = new Invoice { Status = InvoiceStatus.Draft, IsDeleted = true };
        var authorizedNotDeleted = new Invoice { Status = InvoiceStatus.Authorized, IsDeleted = false };
        var paidNotDeleted = new Invoice { Status = InvoiceStatus.Paid, IsDeleted = false };

        // Assert
        draftNotDeleted.IsEditable.Should().BeTrue();
        draftDeleted.IsEditable.Should().BeFalse();
        authorizedNotDeleted.IsEditable.Should().BeFalse();
        paidNotDeleted.IsEditable.Should().BeFalse();
    }

    [Fact]
    public void Invoice_AmountsCanBeZero_ForNewInvoice()
    {
        // Arrange & Act
        var invoice = new Invoice();

        // Assert
        invoice.SubtotalAmount.Should().Be(0);
        invoice.TaxAmount.Should().Be(0);
        invoice.TotalAmount.Should().Be(0);
    }

    [Fact]
    public void Invoice_CanCalculate_TotalFromSubtotalAndTax()
    {
        // Arrange & Act
        var invoice = new Invoice
        {
            SubtotalAmount = 100.00m,
            TaxAmount = 12.00m,
            TotalAmount = 112.00m
        };

        // Assert
        invoice.TotalAmount.Should().Be(invoice.SubtotalAmount + invoice.TaxAmount);
    }

    [Theory]
    [InlineData(SriEnvironment.Test)]
    [InlineData(SriEnvironment.Production)]
    public void Invoice_Environment_CanBeSet(SriEnvironment environment)
    {
        // Arrange & Act
        var invoice = new Invoice { Environment = environment };

        // Assert
        invoice.Environment.Should().Be(environment);
    }

    [Fact]
    public void Invoice_AuthorizationFields_CanBeSetTogether()
    {
        // Arrange
        var authDate = DateTime.UtcNow;
        var auth = "AUTH-123456";

        // Act
        var invoice = new Invoice
        {
            SriAuthorization = auth,
            AuthorizationDate = authDate
        };

        // Assert
        invoice.SriAuthorization.Should().Be(auth);
        invoice.AuthorizationDate.Should().Be(authDate);
    }
}
