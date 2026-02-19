using Xunit;
using FluentAssertions;
using SaaS.Domain.Entities;

namespace Domain.Tests.Entities;

public class ProductTests
{
    [Fact]
    public void Product_ShouldInitialize_WithDefaultValues()
    {
        // Arrange & Act
        var product = new Product();

        // Assert
        product.Name.Should().BeEmpty();
        product.Code.Should().BeEmpty();
        product.SKU.Should().BeEmpty();
        product.Description.Should().BeNull();
        product.Category.Should().BeNull();
        product.Brand.Should().BeNull();
        product.UnitPrice.Should().Be(0);
        product.CostPrice.Should().Be(0);
        product.MinimumStockLevel.Should().Be(0);
        product.Weight.Should().BeNull();
        product.Dimensions.Should().BeNull();
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Product_ShouldSet_AllRequiredProperties()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var product = new Product
        {
            TenantId = tenantId,
            Name = "Test Product",
            Code = "PROD-001",
            SKU = "SKU-001",
            Description = "Test Description",
            Category = "Electronics",
            Brand = "Test Brand",
            UnitPrice = 99.99m,
            CostPrice = 50.00m,
            MinimumStockLevel = 10,
            Weight = 1.5m,
            Dimensions = "10x10x10",
            IsActive = true
        };

        // Assert
        product.TenantId.Should().Be(tenantId);
        product.Name.Should().Be("Test Product");
        product.Code.Should().Be("PROD-001");
        product.SKU.Should().Be("SKU-001");
        product.Description.Should().Be("Test Description");
        product.Category.Should().Be("Electronics");
        product.Brand.Should().Be("Test Brand");
        product.UnitPrice.Should().Be(99.99m);
        product.CostPrice.Should().Be(50.00m);
        product.MinimumStockLevel.Should().Be(10);
        product.Weight.Should().Be(1.5m);
        product.Dimensions.Should().Be("10x10x10");
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Product_ShouldAllowNull_OptionalProperties()
    {
        // Arrange & Act
        var product = new Product
        {
            Name = "Minimal Product",
            Code = "PROD-MIN",
            SKU = "SKU-MIN",
            Description = null,
            Category = null,
            Brand = null,
            Weight = null,
            Dimensions = null
        };

        // Assert
        product.Description.Should().BeNull();
        product.Category.Should().BeNull();
        product.Brand.Should().BeNull();
        product.Weight.Should().BeNull();
        product.Dimensions.Should().BeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Product_IsActive_CanBeSet(bool isActive)
    {
        // Arrange & Act
        var product = new Product { IsActive = isActive };

        // Assert
        product.IsActive.Should().Be(isActive);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(100)]
    public void Product_MinimumStockLevel_CanBeSet(int minimumLevel)
    {
        // Arrange & Act
        var product = new Product { MinimumStockLevel = minimumLevel };

        // Assert
        product.MinimumStockLevel.Should().Be(minimumLevel);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10.50)]
    [InlineData(999.99)]
    public void Product_UnitPrice_CanBeSet(decimal price)
    {
        // Arrange & Act
        var product = new Product { UnitPrice = price };

        // Assert
        product.UnitPrice.Should().Be(price);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5.25)]
    [InlineData(500.00)]
    public void Product_CostPrice_CanBeSet(decimal cost)
    {
        // Arrange & Act
        var product = new Product { CostPrice = cost };

        // Assert
        product.CostPrice.Should().Be(cost);
    }
}
