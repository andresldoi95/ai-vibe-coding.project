using Xunit;
using FluentAssertions;
using SaaS.Domain.Entities;

namespace Domain.Tests.Entities;

public class EmissionPointTests
{
    [Fact]
    public void EmissionPoint_ShouldInitialize_WithDefaultValues()
    {
        // Arrange & Act
        var emissionPoint = new EmissionPoint();

        // Assert
        emissionPoint.EmissionPointCode.Should().BeEmpty();
        emissionPoint.Name.Should().BeEmpty();
        emissionPoint.IsActive.Should().BeTrue();
        emissionPoint.InvoiceSequence.Should().Be(1);
        emissionPoint.CreditNoteSequence.Should().Be(1);
        emissionPoint.DebitNoteSequence.Should().Be(1);
        emissionPoint.RetentionSequence.Should().Be(1);
    }

    [Fact]
    public void EmissionPoint_ShouldSet_AllRequiredProperties()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var establishmentId = Guid.NewGuid();

        // Act
        var emissionPoint = new EmissionPoint
        {
            TenantId = tenantId,
            EmissionPointCode = "001",
            Name = "Main Point",
            IsActive = true,
            InvoiceSequence = 100,
            CreditNoteSequence = 50,
            DebitNoteSequence = 25,
            RetentionSequence = 10,
            EstablishmentId = establishmentId
        };

        // Assert
        emissionPoint.TenantId.Should().Be(tenantId);
        emissionPoint.EmissionPointCode.Should().Be("001");
        emissionPoint.Name.Should().Be("Main Point");
        emissionPoint.IsActive.Should().BeTrue();
        emissionPoint.InvoiceSequence.Should().Be(100);
        emissionPoint.CreditNoteSequence.Should().Be(50);
        emissionPoint.DebitNoteSequence.Should().Be(25);
        emissionPoint.RetentionSequence.Should().Be(10);
        emissionPoint.EstablishmentId.Should().Be(establishmentId);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EmissionPoint_IsActive_CanBeSet(bool isActive)
    {
        // Arrange & Act
        var emissionPoint = new EmissionPoint { IsActive = isActive };

        // Assert
        emissionPoint.IsActive.Should().Be(isActive);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999)]
    public void EmissionPoint_InvoiceSequence_CanBeSet(int sequence)
    {
        // Arrange & Act
        var emissionPoint = new EmissionPoint { InvoiceSequence = sequence };

        // Assert
        emissionPoint.InvoiceSequence.Should().Be(sequence);
    }

    [Theory]
    [InlineData("001")]
    [InlineData("002")]
    [InlineData("999")]
    public void EmissionPoint_EmissionPointCode_CanBeSet(string code)
    {
        // Arrange & Act
        var emissionPoint = new EmissionPoint { EmissionPointCode = code };

        // Assert
        emissionPoint.EmissionPointCode.Should().Be(code);
    }
}
