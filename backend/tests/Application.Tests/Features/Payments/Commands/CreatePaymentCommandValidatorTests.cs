using Xunit;
using FluentAssertions;
using SaaS.Application.Features.Payments.Commands.CreatePayment;
using SaaS.Domain.Enums;

namespace Application.Tests.Features.Payments.Commands;

public class CreatePaymentCommandValidatorTests
{
    private readonly CreatePaymentCommandValidator _validator;

    public CreatePaymentCommandValidatorTests()
    {
        _validator = new CreatePaymentCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreatePaymentCommand
        {
            InvoiceId = Guid.NewGuid(),
            Amount = 500.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = SriPaymentMethod.Cash,
            Status = PaymentStatus.Completed,
            TransactionId = "TXN-12345",
            Notes = "Payment received"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidCommandWithoutOptionalFields_ShouldPass()
    {
        // Arrange
        var command = new CreatePaymentCommand
        {
            InvoiceId = Guid.NewGuid(),
            Amount = 500.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = SriPaymentMethod.Cash,
            Status = PaymentStatus.Pending
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyInvoiceId_ShouldFail()
    {
        // Arrange
        var command = new CreatePaymentCommand
        {
            InvoiceId = Guid.Empty,
            Amount = 500.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = SriPaymentMethod.Cash,
            Status = PaymentStatus.Pending
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "InvoiceId" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_AmountZero_ShouldFail()
    {
        // Arrange
        var command = new CreatePaymentCommand
        {
            InvoiceId = Guid.NewGuid(),
            Amount = 0m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = SriPaymentMethod.Cash,
            Status = PaymentStatus.Pending
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Amount" && e.ErrorMessage.Contains("greater than zero"));
    }

    [Fact]
    public void Validate_AmountNegative_ShouldFail()
    {
        // Arrange
        var command = new CreatePaymentCommand
        {
            InvoiceId = Guid.NewGuid(),
            Amount = -100.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = SriPaymentMethod.Cash,
            Status = PaymentStatus.Pending
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Amount" && e.ErrorMessage.Contains("greater than zero"));
    }

    [Fact]
    public void Validate_AmountVerySmall_ShouldPass()
    {
        // Arrange
        var command = new CreatePaymentCommand
        {
            InvoiceId = Guid.NewGuid(),
            Amount = 0.01m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = SriPaymentMethod.Cash,
            Status = PaymentStatus.Pending
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_PaymentDateDefault_ShouldFail()
    {
        // Arrange
        var command = new CreatePaymentCommand
        {
            InvoiceId = Guid.NewGuid(),
            Amount = 500.00m,
            PaymentDate = default,
            PaymentMethod = SriPaymentMethod.Cash,
            Status = PaymentStatus.Pending
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PaymentDate" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_InvalidPaymentMethod_ShouldFail()
    {
        // Arrange
        var command = new CreatePaymentCommand
        {
            InvoiceId = Guid.NewGuid(),
            Amount = 500.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = (SriPaymentMethod)999,
            Status = PaymentStatus.Pending
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PaymentMethod" && e.ErrorMessage.Contains("Invalid payment method"));
    }

    [Fact]
    public void Validate_InvalidStatus_ShouldFail()
    {
        // Arrange
        var command = new CreatePaymentCommand
        {
            InvoiceId = Guid.NewGuid(),
            Amount = 500.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = SriPaymentMethod.Cash,
            Status = (PaymentStatus)999
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Status" && e.ErrorMessage.Contains("Invalid payment status"));
    }

    [Fact]
    public void Validate_TransactionIdTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreatePaymentCommand
        {
            InvoiceId = Guid.NewGuid(),
            Amount = 500.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = SriPaymentMethod.Cash,
            Status = PaymentStatus.Pending,
            TransactionId = new string('A', 257)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TransactionId" && e.ErrorMessage.Contains("256"));
    }

    [Fact]
    public void Validate_TransactionIdExactly256Characters_ShouldPass()
    {
        // Arrange
        var command = new CreatePaymentCommand
        {
            InvoiceId = Guid.NewGuid(),
            Amount = 500.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = SriPaymentMethod.Cash,
            Status = PaymentStatus.Pending,
            TransactionId = new string('A', 256)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_NotesTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreatePaymentCommand
        {
            InvoiceId = Guid.NewGuid(),
            Amount = 500.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = SriPaymentMethod.Cash,
            Status = PaymentStatus.Pending,
            Notes = new string('A', 1001)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Notes" && e.ErrorMessage.Contains("1000"));
    }

    [Fact]
    public void Validate_NotesExactly1000Characters_ShouldPass()
    {
        // Arrange
        var command = new CreatePaymentCommand
        {
            InvoiceId = Guid.NewGuid(),
            Amount = 500.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = SriPaymentMethod.Cash,
            Status = PaymentStatus.Pending,
            Notes = new string('A', 1000)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
