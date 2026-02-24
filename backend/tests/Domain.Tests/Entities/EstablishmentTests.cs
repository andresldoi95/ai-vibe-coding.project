using Xunit;
using FluentAssertions;
using SaaS.Domain.Entities;

namespace Domain.Tests.Entities;

public class EstablishmentTests
{
    [Fact]
    public void Establishment_ShouldInitialize_WithDefaultValues()
    {
        // Arrange & Act
        var establishment = new Establishment();

        // Assert
        establishment.EstablishmentCode.Should().BeEmpty();
        establishment.Name.Should().BeEmpty();
        establishment.Address.Should().BeEmpty();
        establishment.Phone.Should().BeNull();
        establishment.IsActive.Should().BeTrue();
        establishment.EmissionPoints.Should().BeEmpty();
    }

    [Fact]
    public void Establishment_ShouldSet_AllRequiredProperties()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var establishment = new Establishment
        {
            TenantId = tenantId,
            EstablishmentCode = "001",
            Name = "Main Establishment",
            Address = "123 Main St, City",
            Phone = "+593-2-1234567",
            IsActive = true
        };

        // Assert
        establishment.TenantId.Should().Be(tenantId);
        establishment.EstablishmentCode.Should().Be("001");
        establishment.Name.Should().Be("Main Establishment");
        establishment.Address.Should().Be("123 Main St, City");
        establishment.Phone.Should().Be("+593-2-1234567");
        establishment.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Establishment_ShouldAllowNull_OptionalProperties()
    {
        // Arrange & Act
        var establishment = new Establishment
        {
            EstablishmentCode = "001",
            Name = "Minimal Establishment",
            Address = "123 Street",
            Phone = null
        };

        // Assert
        establishment.Phone.Should().BeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Establishment_IsActive_CanBeSet(bool isActive)
    {
        // Arrange & Act
        var establishment = new Establishment { IsActive = isActive };

        // Assert
        establishment.IsActive.Should().Be(isActive);
    }

    [Theory]
    [InlineData("001", true)]
    [InlineData("002", true)]
    [InlineData("999", true)]
    [InlineData("", false)]
    [InlineData("1", false)]
    [InlineData("12", false)]
    [InlineData("1234", false)]
    [InlineData("000", false)]   // zero is below the valid range (001-999)
    [InlineData("ABC", false)]   // non-numeric
    [InlineData("01A", false)]   // mixed alphanumeric
    public void Establishment_IsValidCode_ValidatesCorrectly(string code, bool expectedValid)
    {
        // Arrange
        var establishment = new Establishment { EstablishmentCode = code };

        // Act
        var result = establishment.IsValidCode();

        // Assert
        result.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData("001")]
    [InlineData("002")]
    [InlineData("999")]
    public void Establishment_EstablishmentCode_CanBeSet(string code)
    {
        // Arrange & Act
        var establishment = new Establishment { EstablishmentCode = code };

        // Assert
        establishment.EstablishmentCode.Should().Be(code);
    }
}
