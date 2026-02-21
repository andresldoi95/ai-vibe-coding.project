using Xunit;
using FluentAssertions;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace Domain.Tests.Entities;

public class EmissionPointBusinessLogicTests
{
    [Theory]
    [InlineData("001", true)]
    [InlineData("999", true)]
    [InlineData("500", true)]
    [InlineData("099", true)]
    public void EmissionPoint_IsValidCode_ReturnsTrueForValidCodes(string code, bool expected)
    {
        // Arrange
        var emissionPoint = new EmissionPoint { EmissionPointCode = code };

        // Act
        var result = emissionPoint.IsValidCode();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("1", false)]    // Too short
    [InlineData("99", false)]   // Too short
    [InlineData("1000", false)] // Too long
    [InlineData("ABC", false)]  // Not digits
    [InlineData("12A", false)]  // Contains letter
    [InlineData("", false)]     // Empty
    [InlineData(null, false)]   // Null
    [InlineData("   ", false)]  // Whitespace
    [InlineData("000", false)]  // Zero (not in range 1-999)
    public void EmissionPoint_IsValidCode_ReturnsFalseForInvalidCodes(string? code, bool expected)
    {
        // Arrange
        var emissionPoint = new EmissionPoint { EmissionPointCode = code! };

        // Act
        var result = emissionPoint.IsValidCode();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void EmissionPoint_GetNextSequential_IncrementsInvoiceSequence()
    {
        // Arrange
        var emissionPoint = new EmissionPoint { InvoiceSequence = 100 };

        // Act
        var sequential = emissionPoint.GetNextSequential(DocumentType.Invoice);

        // Assert
        sequential.Should().Be(100);
        emissionPoint.InvoiceSequence.Should().Be(101);
    }

    [Fact]
    public void EmissionPoint_GetNextSequential_IncrementsCreditNoteSequence()
    {
        // Arrange
        var emissionPoint = new EmissionPoint { CreditNoteSequence = 50 };

        // Act
        var sequential = emissionPoint.GetNextSequential(DocumentType.CreditNote);

        // Assert
        sequential.Should().Be(50);
        emissionPoint.CreditNoteSequence.Should().Be(51);
    }

    [Fact]
    public void EmissionPoint_GetNextSequential_IncrementsDebitNoteSequence()
    {
        // Arrange
        var emissionPoint = new EmissionPoint { DebitNoteSequence = 25 };

        // Act
        var sequential = emissionPoint.GetNextSequential(DocumentType.DebitNote);

        // Assert
        sequential.Should().Be(25);
        emissionPoint.DebitNoteSequence.Should().Be(26);
    }

    [Fact]
    public void EmissionPoint_GetNextSequential_IncrementsRetentionSequence()
    {
        // Arrange
        var emissionPoint = new EmissionPoint { RetentionSequence = 10 };

        // Act
        var sequential = emissionPoint.GetNextSequential(DocumentType.Retention);

        // Assert
        sequential.Should().Be(10);
        emissionPoint.RetentionSequence.Should().Be(11);
    }

    [Fact]
    public void EmissionPoint_GetNextSequential_ThrowsForUnsupportedType()
    {
        // Arrange
        var emissionPoint = new EmissionPoint();
        var invalidType = (DocumentType)999;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            emissionPoint.GetNextSequential(invalidType));
        exception.Message.Should().Contain("Unsupported document type");
    }

    [Fact]
    public void EmissionPoint_GetCurrentSequential_ReturnsInvoiceSequenceWithoutIncrement()
    {
        // Arrange
        var emissionPoint = new EmissionPoint { InvoiceSequence = 100 };

        // Act
        var sequential1 = emissionPoint.GetCurrentSequential(DocumentType.Invoice);
        var sequential2 = emissionPoint.GetCurrentSequential(DocumentType.Invoice);

        // Assert
        sequential1.Should().Be(100);
        sequential2.Should().Be(100);
        emissionPoint.InvoiceSequence.Should().Be(100);
    }

    [Fact]
    public void EmissionPoint_GetCurrentSequential_ReturnsCreditNoteSequenceWithoutIncrement()
    {
        // Arrange
        var emissionPoint = new EmissionPoint { CreditNoteSequence = 50 };

        // Act
        var sequential = emissionPoint.GetCurrentSequential(DocumentType.CreditNote);

        // Assert
        sequential.Should().Be(50);
        emissionPoint.CreditNoteSequence.Should().Be(50);
    }

    [Fact]
    public void EmissionPoint_GetCurrentSequential_ThrowsForUnsupportedType()
    {
        // Arrange
        var emissionPoint = new EmissionPoint();
        var invalidType = (DocumentType)999;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            emissionPoint.GetCurrentSequential(invalidType));
        exception.Message.Should().Contain("Unsupported document type");
    }

    [Fact]
    public void EmissionPoint_SequentialIncrement_CanOccurMultipleTimes()
    {
        // Arrange
        var emissionPoint = new EmissionPoint { InvoiceSequence = 1 };

        // Act
        var seq1 = emissionPoint.GetNextSequential(DocumentType.Invoice);
        var seq2 = emissionPoint.GetNextSequential(DocumentType.Invoice);
        var seq3 = emissionPoint.GetNextSequential(DocumentType.Invoice);

        // Assert
        seq1.Should().Be(1);
        seq2.Should().Be(2);
        seq3.Should().Be(3);
        emissionPoint.InvoiceSequence.Should().Be(4);
    }

    [Fact]
    public void EmissionPoint_DifferentDocumentTypes_HaveIndependentSequences()
    {
        // Arrange
        var emissionPoint = new EmissionPoint
        {
            InvoiceSequence = 10,
            CreditNoteSequence = 20,
            DebitNoteSequence = 30,
            RetentionSequence = 40
        };

        // Act
        var invoice = emissionPoint.GetNextSequential(DocumentType.Invoice);
        var credit = emissionPoint.GetNextSequential(DocumentType.CreditNote);
        var debit = emissionPoint.GetNextSequential(DocumentType.DebitNote);
        var retention = emissionPoint.GetNextSequential(DocumentType.Retention);

        // Assert
        invoice.Should().Be(10);
        credit.Should().Be(20);
        debit.Should().Be(30);
        retention.Should().Be(40);
        emissionPoint.InvoiceSequence.Should().Be(11);
        emissionPoint.CreditNoteSequence.Should().Be(21);
        emissionPoint.DebitNoteSequence.Should().Be(31);
        emissionPoint.RetentionSequence.Should().Be(41);
    }
}
