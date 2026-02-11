using SaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SaaS.Infrastructure.Persistence.Configurations;

public class EstablishmentConfiguration : IEntityTypeConfiguration<Establishment>
{
    public void Configure(EntityTypeBuilder<Establishment> builder)
    {
        builder.ToTable("Establishments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EstablishmentCode)
            .IsRequired()
            .HasMaxLength(3)
            .IsFixedLength();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Phone)
            .HasMaxLength(50);

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(e => new { e.TenantId, e.EstablishmentCode })
            .IsUnique()
            .HasDatabaseName("IX_Establishments_TenantId_Code");

        builder.HasIndex(e => e.TenantId)
            .HasDatabaseName("IX_Establishments_TenantId");

        // Relationships

        builder.HasMany(e => e.EmissionPoints)
            .WithOne(ep => ep.Establishment)
            .HasForeignKey(ep => ep.EstablishmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Audit fields
        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt);

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(100);

        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(100);
    }
}
