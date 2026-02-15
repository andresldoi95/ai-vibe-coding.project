using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.TenantId)
            .IsRequired();

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(w => w.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.Description)
            .HasMaxLength(1000);

        builder.Property(w => w.StreetAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(w => w.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.State)
            .HasMaxLength(100);

        builder.Property(w => w.PostalCode)
            .IsRequired()
            .HasMaxLength(20);

        // Foreign key relationship to Country (required)
        builder.HasOne(w => w.Country)
            .WithMany(c => c.Warehouses)
            .HasForeignKey(w => w.CountryId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(true);

        builder.Property(w => w.Phone)
            .HasMaxLength(50);

        builder.Property(w => w.Email)
            .HasMaxLength(256);

        builder.Property(w => w.SquareFootage)
            .HasColumnType("decimal(18,2)");

        builder.Property(w => w.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(w => new { w.TenantId, w.Code })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false"); // Unique code per tenant, excluding deleted

        builder.HasIndex(w => w.TenantId);

        builder.HasIndex(w => w.IsActive);

        builder.HasIndex(w => w.CountryId);

        // Global query filter for soft delete
        builder.HasQueryFilter(w => !w.IsDeleted);
    }
}
