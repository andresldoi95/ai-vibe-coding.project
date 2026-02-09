using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for WarehouseInventory entity
/// </summary>
public class WarehouseInventoryConfiguration : IEntityTypeConfiguration<WarehouseInventory>
{
    public void Configure(EntityTypeBuilder<WarehouseInventory> builder)
    {
        builder.ToTable("WarehouseInventory");

        builder.HasKey(wi => wi.Id);

        builder.Property(wi => wi.TenantId)
            .IsRequired();

        builder.Property(wi => wi.ProductId)
            .IsRequired();

        builder.Property(wi => wi.WarehouseId)
            .IsRequired();

        builder.Property(wi => wi.Quantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(wi => wi.ReservedQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(wi => wi.LastMovementDate)
            .IsRequired(false);

        // Relationships
        builder.HasOne(wi => wi.Product)
            .WithMany()
            .HasForeignKey(wi => wi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(wi => wi.Warehouse)
            .WithMany()
            .HasForeignKey(wi => wi.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint: One inventory record per product-warehouse-tenant combination
        // Excluding soft-deleted records
        builder.HasIndex(wi => new { wi.TenantId, wi.ProductId, wi.WarehouseId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        // Performance indexes
        builder.HasIndex(wi => wi.TenantId);
        builder.HasIndex(wi => wi.ProductId);
        builder.HasIndex(wi => wi.WarehouseId);
        builder.HasIndex(wi => new { wi.TenantId, wi.ProductId });
        builder.HasIndex(wi => new { wi.TenantId, wi.WarehouseId });

        // Global query filter for soft deletes
        builder.HasQueryFilter(wi => !wi.IsDeleted);
    }
}
