using SaaS.Domain.Entities;
using SaaS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SaaS.Infrastructure.Persistence.Configurations;

public class CreditNoteConfiguration : IEntityTypeConfiguration<CreditNote>
{
    public void Configure(EntityTypeBuilder<CreditNote> builder)
    {
        builder.ToTable("CreditNotes");

        builder.Property(cn => cn.CreditNoteNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(cn => cn.CustomerId)
            .IsRequired();

        builder.Property(cn => cn.IssueDate)
            .IsRequired();

        builder.Property(cn => cn.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(cn => cn.SubtotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(cn => cn.TaxAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(cn => cn.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(cn => cn.ValueModification)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(cn => cn.Reason)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(cn => cn.Notes)
            .HasMaxLength(2000);

        builder.Property(cn => cn.OriginalInvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(cn => cn.OriginalInvoiceDate)
            .IsRequired();

        // SRI Fields
        builder.Property(cn => cn.DocumentType)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(DocumentType.CreditNote);

        builder.Property(cn => cn.AccessKey)
            .HasMaxLength(49);

        builder.Property(cn => cn.PaymentMethod)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(SriPaymentMethod.Cash);

        builder.Property(cn => cn.XmlFilePath)
            .HasMaxLength(500);

        builder.Property(cn => cn.SignedXmlFilePath)
            .HasMaxLength(500);

        builder.Property(cn => cn.RideFilePath)
            .HasMaxLength(500);

        builder.Property(cn => cn.Environment)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(SriEnvironment.Test);

        builder.Property(cn => cn.SriAuthorization)
            .HasMaxLength(49);

        // Relationships
        builder.HasOne(cn => cn.Customer)
            .WithMany()
            .HasForeignKey(cn => cn.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cn => cn.EmissionPoint)
            .WithMany(ep => ep.CreditNotes)
            .HasForeignKey(cn => cn.EmissionPointId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cn => cn.OriginalInvoice)
            .WithMany()
            .HasForeignKey(cn => cn.OriginalInvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(cn => cn.Items)
            .WithOne(ci => ci.CreditNote)
            .HasForeignKey(ci => ci.CreditNoteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(cn => new { cn.TenantId, cn.CreditNoteNumber })
            .IsUnique()
            .HasDatabaseName("IX_CreditNotes_TenantId_CreditNoteNumber");

        builder.HasIndex(cn => new { cn.TenantId, cn.CustomerId })
            .HasDatabaseName("IX_CreditNotes_TenantId_CustomerId");

        builder.HasIndex(cn => new { cn.TenantId, cn.IssueDate })
            .HasDatabaseName("IX_CreditNotes_TenantId_IssueDate");

        builder.HasIndex(cn => new { cn.TenantId, cn.Status })
            .HasDatabaseName("IX_CreditNotes_TenantId_Status");

        builder.HasIndex(cn => cn.TenantId)
            .HasDatabaseName("IX_CreditNotes_TenantId");

        builder.HasIndex(cn => cn.AccessKey)
            .IsUnique()
            .HasDatabaseName("IX_CreditNotes_AccessKey")
            .HasFilter("\"AccessKey\" IS NOT NULL");

        builder.HasIndex(cn => cn.EmissionPointId)
            .HasDatabaseName("IX_CreditNotes_EmissionPointId");

        builder.HasIndex(cn => cn.OriginalInvoiceId)
            .HasDatabaseName("IX_CreditNotes_OriginalInvoiceId");

        // Query filter for soft delete
        builder.HasQueryFilter(cn => !cn.IsDeleted);
    }
}
