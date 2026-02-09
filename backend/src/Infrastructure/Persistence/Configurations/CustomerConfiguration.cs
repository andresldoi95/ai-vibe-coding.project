using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.TenantId)
            .IsRequired();

        // Basic Information
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(c => c.Phone)
            .HasMaxLength(50);

        builder.Property(c => c.TaxId)
            .HasMaxLength(50);

        builder.Property(c => c.ContactPerson)
            .HasMaxLength(256);

        // Billing Address
        builder.Property(c => c.BillingStreet)
            .HasMaxLength(512);

        builder.Property(c => c.BillingCity)
            .HasMaxLength(100);

        builder.Property(c => c.BillingState)
            .HasMaxLength(100);

        builder.Property(c => c.BillingPostalCode)
            .HasMaxLength(20);

        builder.Property(c => c.BillingCountry)
            .HasMaxLength(100);

        // Shipping Address
        builder.Property(c => c.ShippingStreet)
            .HasMaxLength(512);

        builder.Property(c => c.ShippingCity)
            .HasMaxLength(100);

        builder.Property(c => c.ShippingState)
            .HasMaxLength(100);

        builder.Property(c => c.ShippingPostalCode)
            .HasMaxLength(20);

        builder.Property(c => c.ShippingCountry)
            .HasMaxLength(100);

        // Additional Information
        builder.Property(c => c.Notes)
            .HasMaxLength(2000);

        builder.Property(c => c.Website)
            .HasMaxLength(256);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Unique constraint: Email must be unique per tenant (excluding soft-deleted)
        builder.HasIndex(c => new { c.TenantId, c.Email })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        // Unique constraint: TaxId must be unique per tenant when provided (excluding soft-deleted)
        builder.HasIndex(c => new { c.TenantId, c.TaxId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false AND \"TaxId\" IS NOT NULL");

        // Performance indexes
        builder.HasIndex(c => c.TenantId);
        builder.HasIndex(c => c.IsActive);
        builder.HasIndex(c => c.BillingCity);
        builder.HasIndex(c => c.BillingCountry);

        // Global query filter for soft delete
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
