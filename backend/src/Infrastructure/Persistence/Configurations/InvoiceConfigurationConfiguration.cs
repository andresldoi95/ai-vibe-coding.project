using SaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SaaS.Infrastructure.Persistence.Configurations;

public class InvoiceConfigurationConfiguration : IEntityTypeConfiguration<InvoiceConfiguration>
{
    public void Configure(EntityTypeBuilder<InvoiceConfiguration> builder)
    {
        builder.ToTable("InvoiceConfigurations");

        builder.Property(ic => ic.EstablishmentCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(ic => ic.EmissionPointCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(ic => ic.NextSequentialNumber)
            .IsRequired();

        builder.Property(ic => ic.DueDays)
            .IsRequired();

        // Relationships
        builder.HasOne(ic => ic.DefaultTaxRate)
            .WithMany()
            .HasForeignKey(ic => ic.DefaultTaxRateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ic => ic.DefaultWarehouse)
            .WithMany()
            .HasForeignKey(ic => ic.DefaultWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint - one configuration per tenant
        builder.HasIndex(ic => ic.TenantId)
            .IsUnique()
            .HasDatabaseName("IX_InvoiceConfigurations_TenantId_Unique");

        // Query filter for soft delete
        builder.HasQueryFilter(ic => !ic.IsDeleted);
    }
}
