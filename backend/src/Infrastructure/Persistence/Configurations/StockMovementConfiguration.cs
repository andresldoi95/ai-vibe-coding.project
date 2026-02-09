using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace SaaS.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for StockMovement entity
/// </summary>
public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");

        // Properties
        builder.Property(sm => sm.MovementType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(sm => sm.ProductId)
            .IsRequired();

        builder.Property(sm => sm.WarehouseId)
            .IsRequired();

        builder.Property(sm => sm.DestinationWarehouseId)
            .IsRequired(false);

        builder.Property(sm => sm.Quantity)
            .IsRequired();

        builder.Property(sm => sm.UnitCost)
            .HasPrecision(18, 2)
            .IsRequired(false);

        builder.Property(sm => sm.TotalCost)
            .HasPrecision(18, 2)
            .IsRequired(false);

        builder.Property(sm => sm.Reference)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(sm => sm.Notes)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(sm => sm.MovementDate)
            .IsRequired();

        // Relationships
        builder.HasOne(sm => sm.Product)
            .WithMany()
            .HasForeignKey(sm => sm.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sm => sm.Warehouse)
            .WithMany()
            .HasForeignKey(sm => sm.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sm => sm.DestinationWarehouse)
            .WithMany()
            .HasForeignKey(sm => sm.DestinationWarehouseId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Indexes for performance
        builder.HasIndex(sm => sm.TenantId);
        builder.HasIndex(sm => sm.ProductId);
        builder.HasIndex(sm => sm.WarehouseId);
        builder.HasIndex(sm => sm.DestinationWarehouseId);
        builder.HasIndex(sm => sm.MovementType);
        builder.HasIndex(sm => sm.MovementDate);
        builder.HasIndex(sm => new { sm.TenantId, sm.ProductId });
        builder.HasIndex(sm => new { sm.TenantId, sm.WarehouseId });

        // Global query filter for soft deletes
        builder.HasQueryFilter(sm => !sm.IsDeleted);
    }
}
