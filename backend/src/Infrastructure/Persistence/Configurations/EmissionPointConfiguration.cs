using SaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SaaS.Infrastructure.Persistence.Configurations;

public class EmissionPointConfiguration : IEntityTypeConfiguration<EmissionPoint>
{
    public void Configure(EntityTypeBuilder<EmissionPoint> builder)
    {
        builder.ToTable("EmissionPoints");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EmissionPointCode)
            .IsRequired()
            .HasMaxLength(3)
            .IsFixedLength();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.InvoiceSequence)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(e => e.CreditNoteSequence)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(e => e.DebitNoteSequence)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(e => e.RetentionSequence)
            .IsRequired()
            .HasDefaultValue(1);

        // Indexes
        builder.HasIndex(e => new { e.EstablishmentId, e.EmissionPointCode })
            .IsUnique()
            .HasDatabaseName("IX_EmissionPoints_EstablishmentId_Code");

        builder.HasIndex(e => e.EstablishmentId)
            .HasDatabaseName("IX_EmissionPoints_EstablishmentId");

        // Relationships
        builder.HasOne(e => e.Establishment)
            .WithMany(es => es.EmissionPoints)
            .HasForeignKey(e => e.EstablishmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Invoices)
            .WithOne(i => i.EmissionPoint)
            .HasForeignKey(i => i.EmissionPointId)
            .OnDelete(DeleteBehavior.Restrict);

        // Audit fields
        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt);

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(100);

        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(100);

        // Concurrency token for sequential number updates
        builder.Property(e => e.InvoiceSequence)
            .IsConcurrencyToken();

        builder.Property(e => e.CreditNoteSequence)
            .IsConcurrencyToken();

        builder.Property(e => e.DebitNoteSequence)
            .IsConcurrencyToken();

        builder.Property(e => e.RetentionSequence)
            .IsConcurrencyToken();
    }
}
