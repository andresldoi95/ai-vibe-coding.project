using Xunit;
using FluentAssertions;
using SaaS.Domain.Entities;
using SaaS.Domain.Common;

namespace Domain.Tests.Entities;

public class EstablishmentBusinessLogicTests
{
    [Theory]
    [InlineData("001", true)]  // Valid minimum
    [InlineData("002", true)]  // Valid low
    [InlineData("100", true)]  // Valid mid
    [InlineData("500", true)]  // Valid mid-high
    [InlineData("999", true)]  // Valid maximum
    public void Establishment_IsValidCode_ReturnsTrueForValidCodes(string code, bool expected)
    {
        // Arrange
        var establishment = new Establishment { EstablishmentCode = code };

        // Act
        var result = establishment.IsValidCode();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("000", false)]  // Invalid - zero
    [InlineData("00", false)]   // Invalid - too short
    [InlineData("0001", false)] // Invalid - too long
    [InlineData("ABC", false)]  // Invalid - letters
    [InlineData("1A2", false)]  // Invalid - mixed
    [InlineData("", false)]     // Invalid - empty
    [InlineData(null, false)]   // Invalid - null
    [InlineData("   ", false)]  // Invalid - whitespace
    [InlineData("-01", false)]  // Invalid - negative sign
    [InlineData("1.0", false)]  // Invalid - decimal
    public void Establishment_IsValidCode_ReturnsFalseForInvalidCodes(string? code, bool expected)
    {
        // Arrange
        var establishment = new Establishment { EstablishmentCode = code! };

        // Act
        var result = establishment.IsValidCode();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Establishment_IsValidCode_ReturnsFalseForWhitespaceOnly()
    {
        // Arrange
        var establishment = new Establishment { EstablishmentCode = "   " };

        // Act
        var result = establishment.IsValidCode();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Establishment_IsValidCode_ValidatesExactLength()
    {
        // Arrange
        var tooShort = new Establishment { EstablishmentCode = "01" };
        var tooLong = new Establishment { EstablishmentCode = "0001" };
        var justRight = new Establishment { EstablishmentCode = "001" };

        // Act & Assert
        tooShort.IsValidCode().Should().BeFalse();
        tooLong.IsValidCode().Should().BeFalse();
        justRight.IsValidCode().Should().BeTrue();
    }

    [Fact]
    public void Establishment_IsValidCode_RequiresNumericOnly()
    {
        // Arrange
        var allNumeric = new Establishment { EstablishmentCode = "123" };
        var withLetter = new Establishment { EstablishmentCode = "12A" };
        var withSymbol = new Establishment { EstablishmentCode = "12@" };

        // Act & Assert
        allNumeric.IsValidCode().Should().BeTrue();
        withLetter.IsValidCode().Should().BeFalse();
        withSymbol.IsValidCode().Should().BeFalse();
    }

    [Fact]
    public void Establishment_IsValidCode_RejectsZeroCode()
    {
        // Arrange
        var establishment = new Establishment { EstablishmentCode = "000" };

        // Act
        var result = establishment.IsValidCode();

        // Assert
        result.Should().BeFalse("code 000 is not a valid establishment code");
    }

    [Theory]
    [InlineData("001")]
    [InlineData("010")]
    [InlineData("100")]
    public void Establishment_IsValidCode_AcceptsLeadingZeros(string code)
    {
        // Arrange
        var establishment = new Establishment { EstablishmentCode = code };

        // Act
        var result = establishment.IsValidCode();

        // Assert
        result.Should().BeTrue($"code {code} should be valid with leading zeros");
    }

    [Fact]
    public void Establishment_HasNameProperty()
    {
        // Arrange
        var name = "Main Office";
        var establishment = new Establishment { Name = name };

        // Act & Assert
        establishment.Name.Should().Be(name);
        establishment.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Establishment_InheritsFromTenantEntity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var establishment = new Establishment { TenantId = tenantId };

        // Assert
        establishment.Should().BeAssignableTo<TenantEntity>();
        establishment.TenantId.Should().Be(tenantId);
    }
}
