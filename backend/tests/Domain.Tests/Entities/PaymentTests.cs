using Xunit;
using FluentAssertions;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace Domain.Tests.Entities;

public class PaymentTests
{
    [Fact]
    public void Payment_ShouldInitialize_WithDefaultValues()
    {
        // Arrange & Act
        var payment = new Payment();

        // Assert
        payment.InvoiceId.Should().BeEmpty();
        payment.Amount.Should().Be(0);
        payment.PaymentDate.Should().Be(default(DateTime));
        payment.PaymentMethod.Should().Be(default(SriPaymentMethod));
        payment.Status.Should().Be(default(PaymentStatus));
        payment.TransactionId.Should().BeNull();
        payment.Notes.Should().BeNull();
    }

    [Fact]
    public void Payment_ShouldSet_AllRequiredProperties()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var paymentDate = DateTime.UtcNow;

        // Act
        var payment = new Payment
        {
            TenantId = tenantId,
            InvoiceId = invoiceId,
            Amount = 100.00m,
            PaymentDate = paymentDate,
            PaymentMethod = SriPaymentMethod.CreditCard,
            Status = PaymentStatus.Completed,
            TransactionId = "TXN-123456",
            Notes = "Test payment"
        };

        // Assert
        payment.TenantId.Should().Be(tenantId);
        payment.InvoiceId.Should().Be(invoiceId);
        payment.Amount.Should().Be(100.00m);
        payment.PaymentDate.Should().Be(paymentDate);
        payment.PaymentMethod.Should().Be(SriPaymentMethod.CreditCard);
        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.TransactionId.Should().Be("TXN-123456");
        payment.Notes.Should().Be("Test payment");
    }

    [Fact]
    public void Payment_ShouldAllowNull_OptionalProperties()
    {
        // Arrange & Act
        var payment = new Payment
        {
            InvoiceId = Guid.NewGuid(),
            Amount = 50.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = SriPaymentMethod.Cash,
            Status = PaymentStatus.Pending,
            TransactionId = null,
            Notes = null
        };

        // Assert
        payment.TransactionId.Should().BeNull();
        payment.Notes.Should().BeNull();
    }

    [Theory]
    [InlineData(PaymentStatus.Pending)]
    [InlineData(PaymentStatus.Completed)]
    [InlineData(PaymentStatus.Voided)]
    public void Payment_Status_CanBeSet(PaymentStatus status)
    {
        // Arrange & Act
        var payment = new Payment { Status = status };

        // Assert
        payment.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(SriPaymentMethod.Cash)]
    [InlineData(SriPaymentMethod.CreditCard)]
    [InlineData(SriPaymentMethod.DebitCard)]
    [InlineData(SriPaymentMethod.Check)]
    [InlineData(SriPaymentMethod.BankTransfer)]
    public void Payment_PaymentMethod_CanBeSet(SriPaymentMethod paymentMethod)
    {
        // Arrange & Act
        var payment = new Payment { PaymentMethod = paymentMethod };

        // Assert
        payment.PaymentMethod.Should().Be(paymentMethod);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50.00)]
    [InlineData(1000.00)]
    public void Payment_Amount_CanBeSet(decimal amount)
    {
        // Arrange & Act
        var payment = new Payment { Amount = amount };

        // Assert
        payment.Amount.Should().Be(amount);
    }

    [Fact]
    public void Payment_WithInvoiceRelationship_ShouldHaveInvoiceId()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();

        // Act
        var payment = new Payment { InvoiceId = invoiceId };

        // Assert
        payment.InvoiceId.Should().Be(invoiceId);
        payment.InvoiceId.Should().NotBeEmpty();
    }

    [Fact]
    public void Payment_Complete_ChangesStatusAndSetsDate()
    {
        // Arrange
        var paymentDate = DateTime.UtcNow;
        var payment = new Payment
        {
            Status = PaymentStatus.Pending,
            Amount = 100.00m,
            PaymentMethod = SriPaymentMethod.Cash
        };

        // Act
        payment.Status = PaymentStatus.Completed;
        payment.PaymentDate = paymentDate;

        // Assert
        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.PaymentDate.Should().Be(paymentDate);
    }

    [Fact]
    public void Payment_Void_ChangesStatus()
    {
        // Arrange
        var payment = new Payment
        {
            Status = PaymentStatus.Completed,
            Amount = 100.00m
        };

        // Act
        payment.Status = PaymentStatus.Voided;

        // Assert
        payment.Status.Should().Be(PaymentStatus.Voided);
    }

    [Fact]
    public void Payment_TransactionId_CanStoreExternalReference()
    {
        // Arrange
        var transactionId = "STRIPE-TXN-123456789";

        // Act
        var payment = new Payment { TransactionId = transactionId };

        // Assert
        payment.TransactionId.Should().Be(transactionId);
    }

    [Fact]
    public void Payment_StatusTransition_FromPendingToCompleted()
    {
        // Arrange
        var payment = new Payment { Status = PaymentStatus.Pending };

        // Act
        payment.Status = PaymentStatus.Completed;

        // Assert
        payment.Status.Should().Be(PaymentStatus.Completed);
    }

    [Fact]
    public void Payment_PaymentDate_CanBeSetToAnyDateTime()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-30);
        var futureDate = DateTime.UtcNow.AddDays(30);

        // Act
        var payment1 = new Payment { PaymentDate = pastDate };
        var payment2 = new Payment { PaymentDate = futureDate };

        // Assert
        payment1.PaymentDate.Should().Be(pastDate);
        payment2.PaymentDate.Should().Be(futureDate);
    }
}
