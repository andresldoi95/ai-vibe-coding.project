using Xunit;
using FluentAssertions;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace Domain.Tests.Entities;

public class CustomerTests
{
    [Fact]
    public void Customer_ShouldInitialize_WithDefaultValues()
    {
        // Arrange & Act
        var customer = new Customer();

        // Assert
        customer.Name.Should().BeEmpty();
        customer.Email.Should().BeEmpty();
        customer.Phone.Should().BeNull();
        customer.IdentificationType.Should().Be(IdentificationType.Cedula);
        customer.TaxId.Should().BeNull();
        customer.ContactPerson.Should().BeNull();
        customer.BillingStreet.Should().BeNull();
        customer.BillingCity.Should().BeNull();
        customer.BillingState.Should().BeNull();
        customer.BillingPostalCode.Should().BeNull();
        customer.BillingCountryId.Should().BeNull();
        customer.ShippingStreet.Should().BeNull();
        customer.ShippingCity.Should().BeNull();
        customer.ShippingState.Should().BeNull();
        customer.ShippingPostalCode.Should().BeNull();
        customer.ShippingCountryId.Should().BeNull();
        customer.Notes.Should().BeNull();
        customer.Website.Should().BeNull();
        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Customer_ShouldSet_AllRequiredProperties()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var billingCountryId = Guid.NewGuid();
        var shippingCountryId = Guid.NewGuid();

        // Act
        var customer = new Customer
        {
            TenantId = tenantId,
            Name = "Test Customer",
            Email = "customer@example.com",
            Phone = "+1-555-0100",
            IdentificationType = IdentificationType.Ruc,
            TaxId = "1234567890001",
            ContactPerson = "John Doe",
            BillingStreet = "123 Billing St",
            BillingCity = "New York",
            BillingState = "NY",
            BillingPostalCode = "10001",
            BillingCountryId = billingCountryId,
            ShippingStreet = "456 Shipping Ave",
            ShippingCity = "Boston",
            ShippingState = "MA",
            ShippingPostalCode = "02101",
            ShippingCountryId = shippingCountryId,
            Notes = "Test notes",
            Website = "https://example.com",
            IsActive = true
        };

        // Assert
        customer.TenantId.Should().Be(tenantId);
        customer.Name.Should().Be("Test Customer");
        customer.Email.Should().Be("customer@example.com");
        customer.Phone.Should().Be("+1-555-0100");
        customer.IdentificationType.Should().Be(IdentificationType.Ruc);
        customer.TaxId.Should().Be("1234567890001");
        customer.ContactPerson.Should().Be("John Doe");
        customer.BillingStreet.Should().Be("123 Billing St");
        customer.BillingCity.Should().Be("New York");
        customer.BillingState.Should().Be("NY");
        customer.BillingPostalCode.Should().Be("10001");
        customer.BillingCountryId.Should().Be(billingCountryId);
        customer.ShippingStreet.Should().Be("456 Shipping Ave");
        customer.ShippingCity.Should().Be("Boston");
        customer.ShippingState.Should().Be("MA");
        customer.ShippingPostalCode.Should().Be("02101");
        customer.ShippingCountryId.Should().Be(shippingCountryId);
        customer.Notes.Should().Be("Test notes");
        customer.Website.Should().Be("https://example.com");
        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Customer_ShouldAllowNull_OptionalProperties()
    {
        // Arrange & Act
        var customer = new Customer
        {
            Name = "Minimal Customer",
            Email = "minimal@example.com",
            Phone = null,
            TaxId = null,
            ContactPerson = null,
            Notes = null,
            Website = null
        };

        // Assert
        customer.Phone.Should().BeNull();
        customer.TaxId.Should().BeNull();
        customer.ContactPerson.Should().BeNull();
        customer.Notes.Should().BeNull();
        customer.Website.Should().BeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Customer_IsActive_CanBeSet(bool isActive)
    {
        // Arrange & Act
        var customer = new Customer { IsActive = isActive };

        // Assert
        customer.IsActive.Should().Be(isActive);
    }

    [Theory]
    [InlineData(IdentificationType.Cedula)]
    [InlineData(IdentificationType.Ruc)]
    [InlineData(IdentificationType.Passport)]
    public void Customer_IdentificationType_CanBeSet(IdentificationType identificationType)
    {
        // Arrange & Act
        var customer = new Customer { IdentificationType = identificationType };

        // Assert
        customer.IdentificationType.Should().Be(identificationType);
    }
}
