using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.TenantId)
            .IsRequired();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.SKU)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Category)
            .HasMaxLength(100);

        builder.Property(p => p.Brand)
            .HasMaxLength(100);

        builder.Property(p => p.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.CostPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.MinimumStockLevel)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.Weight)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Dimensions)
            .HasMaxLength(100);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Unique constraint: Code must be unique per tenant (excluding soft-deleted)
        builder.HasIndex(p => new { p.TenantId, p.Code })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        // Unique constraint: SKU must be unique per tenant (excluding soft-deleted)
        builder.HasIndex(p => new { p.TenantId, p.SKU })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        // Performance indexes
        builder.HasIndex(p => p.TenantId);
        builder.HasIndex(p => p.IsActive);
        builder.HasIndex(p => p.Category);
        builder.HasIndex(p => p.Brand);

        // Global query filter for soft delete
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
