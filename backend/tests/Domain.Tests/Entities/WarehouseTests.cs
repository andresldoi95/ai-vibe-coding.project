using Xunit;
using FluentAssertions;
using SaaS.Domain.Entities;

namespace Domain.Tests.Entities;

public class WarehouseTests
{
    [Fact]
    public void Warehouse_ShouldInitialize_WithDefaultValues()
    {
        // Arrange & Act
        var warehouse = new Warehouse();

        // Assert
        warehouse.Name.Should().BeEmpty();
        warehouse.Code.Should().BeEmpty();
        warehouse.Description.Should().BeNull();
        warehouse.StreetAddress.Should().BeEmpty();
        warehouse.City.Should().BeEmpty();
        warehouse.State.Should().BeNull();
        warehouse.PostalCode.Should().BeEmpty();
        warehouse.Country.Should().BeEmpty();
        warehouse.Phone.Should().BeNull();
        warehouse.Email.Should().BeNull();
        warehouse.IsActive.Should().BeTrue();
        warehouse.SquareFootage.Should().BeNull();
        warehouse.Capacity.Should().BeNull();
    }

    [Fact]
    public void Warehouse_ShouldSet_AllRequiredProperties()
    {
        // Arrange & Act
        var warehouse = new Warehouse
        {
            TenantId = Guid.NewGuid(),
            Name = "Main Warehouse",
            Code = "WH-001",
            Description = "Primary storage facility",
            StreetAddress = "123 Storage St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "USA",
            Phone = "+1-555-0100",
            Email = "warehouse@example.com",
            IsActive = true,
            SquareFootage = 50000m,
            Capacity = 10000
        };

        // Assert
        warehouse.TenantId.Should().NotBeEmpty();
        warehouse.Name.Should().Be("Main Warehouse");
        warehouse.Code.Should().Be("WH-001");
        warehouse.Description.Should().Be("Primary storage facility");
        warehouse.StreetAddress.Should().Be("123 Storage St");
        warehouse.City.Should().Be("New York");
        warehouse.State.Should().Be("NY");
        warehouse.PostalCode.Should().Be("10001");
        warehouse.Country.Should().Be("USA");
        warehouse.Phone.Should().Be("+1-555-0100");
        warehouse.Email.Should().Be("warehouse@example.com");
        warehouse.IsActive.Should().BeTrue();
        warehouse.SquareFootage.Should().Be(50000m);
        warehouse.Capacity.Should().Be(10000);
    }

    [Fact]
    public void Warehouse_ShouldAllowNull_OptionalProperties()
    {
        // Arrange & Act
        var warehouse = new Warehouse
        {
            Name = "Minimal Warehouse",
            Code = "WH-MIN",
            Description = null,
            StreetAddress = "456 Simple Ave",
            City = "Boston",
            State = null,
            PostalCode = "02101",
            Country = "USA",
            Phone = null,
            Email = null,
            SquareFootage = null,
            Capacity = null
        };

        // Assert
        warehouse.Description.Should().BeNull();
        warehouse.State.Should().BeNull();
        warehouse.Phone.Should().BeNull();
        warehouse.Email.Should().BeNull();
        warehouse.SquareFootage.Should().BeNull();
        warehouse.Capacity.Should().BeNull();
    }

    [Fact]
    public void Warehouse_IsActive_ShouldDefault_ToTrue()
    {
        // Arrange & Act
        var warehouse = new Warehouse
        {
            Name = "Test Warehouse",
            Code = "TEST-WH"
        };

        // Assert
        warehouse.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Warehouse_IsActive_CanBeSet_ToAnyBooleanValue(bool isActive)
    {
        // Arrange & Act
        var warehouse = new Warehouse
        {
            Name = "Test Warehouse",
            Code = "TEST-WH",
            IsActive = isActive
        };

        // Assert
        warehouse.IsActive.Should().Be(isActive);
    }

    [Theory]
    [InlineData(1000.50)]
    [InlineData(0)]
    [InlineData(999999.99)]
    public void Warehouse_SquareFootage_CanBeSet_ToValidDecimal(decimal squareFootage)
    {
        // Arrange & Act
        var warehouse = new Warehouse
        {
            Name = "Test Warehouse",
            Code = "TEST-WH",
            SquareFootage = squareFootage
        };

        // Assert
        warehouse.SquareFootage.Should().Be(squareFootage);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(0)]
    [InlineData(50000)]
    public void Warehouse_Capacity_CanBeSet_ToValidInteger(int capacity)
    {
        // Arrange & Act
        var warehouse = new Warehouse
        {
            Name = "Test Warehouse",
            Code = "TEST-WH",
            Capacity = capacity
        };

        // Assert
        warehouse.Capacity.Should().Be(capacity);
    }
}
