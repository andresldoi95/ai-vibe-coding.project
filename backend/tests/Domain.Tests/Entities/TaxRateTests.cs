using Xunit;
using FluentAssertions;
using SaaS.Domain.Entities;

namespace Domain.Tests.Entities;

public class TaxRateTests
{
    [Fact]
    public void TaxRate_ShouldInitialize_WithDefaultValues()
    {
        // Arrange & Act
        var taxRate = new TaxRate();

        // Assert
        taxRate.Code.Should().BeEmpty();
        taxRate.Name.Should().BeEmpty();
        taxRate.Rate.Should().Be(0);
        taxRate.IsDefault.Should().BeFalse();
        taxRate.IsActive.Should().BeTrue();
        taxRate.Description.Should().BeNull();
        taxRate.CountryId.Should().BeNull();
    }

    [Fact]
    public void TaxRate_ShouldSet_AllRequiredProperties()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var countryId = Guid.NewGuid();

        // Act
        var taxRate = new TaxRate
        {
            TenantId = tenantId,
            Code = "IVA12",
            Name = "IVA 12%",
            Rate = 12.00m,
            IsDefault = true,
            IsActive = true,
            Description = "Standard VAT rate",
            CountryId = countryId
        };

        // Assert
        taxRate.TenantId.Should().Be(tenantId);
        taxRate.Code.Should().Be("IVA12");
        taxRate.Name.Should().Be("IVA 12%");
        taxRate.Rate.Should().Be(12.00m);
        taxRate.IsDefault.Should().BeTrue();
        taxRate.IsActive.Should().BeTrue();
        taxRate.Description.Should().Be("Standard VAT rate");
        taxRate.CountryId.Should().Be(countryId);
    }

    [Fact]
    public void TaxRate_ShouldAllowNull_OptionalProperties()
    {
        // Arrange & Act
        var taxRate = new TaxRate
        {
            Code = "IVA0",
            Name = "IVA 0%",
            Rate = 0,
            Description = null,
            CountryId = null
        };

        // Assert
        taxRate.Description.Should().BeNull();
        taxRate.CountryId.Should().BeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TaxRate_IsActive_CanBeSet(bool isActive)
    {
        // Arrange & Act
        var taxRate = new TaxRate { IsActive = isActive };

        // Assert
        taxRate.IsActive.Should().Be(isActive);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TaxRate_IsDefault_CanBeSet(bool isDefault)
    {
        // Arrange & Act
        var taxRate = new TaxRate { IsDefault = isDefault };

        // Assert
        taxRate.IsDefault.Should().Be(isDefault);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(12.00)]
    [InlineData(15.00)]
    [InlineData(21.00)]
    public void TaxRate_Rate_CanBeSet(decimal rate)
    {
        // Arrange & Act
        var taxRate = new TaxRate { Rate = rate };

        // Assert
        taxRate.Rate.Should().Be(rate);
    }
}
