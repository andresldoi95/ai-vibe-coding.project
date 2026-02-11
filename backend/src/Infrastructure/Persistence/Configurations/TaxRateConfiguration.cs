using SaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SaaS.Infrastructure.Persistence.Configurations;

public class TaxRateConfiguration : IEntityTypeConfiguration<TaxRate>
{
    public void Configure(EntityTypeBuilder<TaxRate> builder)
    {
        builder.ToTable("TaxRates");

        builder.Property(tr => tr.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(tr => tr.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(tr => tr.Rate)
            .IsRequired()
            .HasPrecision(5, 4); // Supports 0.0000 to 9.9999

        builder.Property(tr => tr.Description)
            .HasMaxLength(500);

        builder.Property(tr => tr.Country)
            .HasMaxLength(2); // ISO country code

        // Indexes
        builder.HasIndex(tr => new { tr.TenantId, tr.Code })
            .IsUnique()
            .HasDatabaseName("IX_TaxRates_TenantId_Code");

        builder.HasIndex(tr => tr.TenantId)
            .HasDatabaseName("IX_TaxRates_TenantId");

        builder.HasIndex(tr => tr.IsActive)
            .HasDatabaseName("IX_TaxRates_IsActive");

        builder.HasIndex(tr => tr.Country)
            .HasDatabaseName("IX_TaxRates_Country");

        // Query filter for soft delete
        builder.HasQueryFilter(tr => !tr.IsDeleted);
    }
}
