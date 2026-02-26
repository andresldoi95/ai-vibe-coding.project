using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Configurations;

public class SriErrorLogConfiguration : IEntityTypeConfiguration<SriErrorLog>
{
    public void Configure(EntityTypeBuilder<SriErrorLog> builder)
    {
        builder.ToTable("SriErrorLogs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Operation)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.ErrorCode)
            .HasMaxLength(20);

        builder.Property(e => e.ErrorMessage)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.StackTrace)
            .HasColumnType("text");

        builder.Property(e => e.AdditionalData)
            .HasColumnType("text");

        builder.Property(e => e.OccurredAt)
            .IsRequired();

        builder.Property(e => e.WasRetried)
            .HasDefaultValue(false);

        // Indexes for performance
        builder.HasIndex(e => e.InvoiceId);
        builder.HasIndex(e => e.CreditNoteId);
        builder.HasIndex(e => e.Operation);
        builder.HasIndex(e => e.OccurredAt);
        builder.HasIndex(e => new { e.TenantId, e.OccurredAt });

        // Relationships
        builder.HasOne(e => e.Invoice)
            .WithMany()
            .HasForeignKey(e => e.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CreditNote)
            .WithMany()
            .HasForeignKey(e => e.CreditNoteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Soft delete filter
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
