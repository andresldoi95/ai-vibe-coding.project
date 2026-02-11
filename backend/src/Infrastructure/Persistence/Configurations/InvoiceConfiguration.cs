using SaaS.Domain.Entities;
using SaaS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SaaS.Infrastructure.Persistence.Configurations;

public class InvoiceEntityConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.Property(i => i.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.CustomerId)
            .IsRequired();

        builder.Property(i => i.IssueDate)
            .IsRequired();

        builder.Property(i => i.DueDate)
            .IsRequired();

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<int>(); // Store enum as int

        builder.Property(i => i.SubtotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(i => i.TaxAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(i => i.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(i => i.Notes)
            .HasMaxLength(2000);

        // SRI Fields
        builder.Property(i => i.DocumentType)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(DocumentType.Invoice);

        builder.Property(i => i.AccessKey)
            .HasMaxLength(49);

        builder.Property(i => i.PaymentMethod)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(SriPaymentMethod.Cash);

        builder.Property(i => i.XmlFilePath)
            .HasMaxLength(500);

        builder.Property(i => i.SignedXmlFilePath)
            .HasMaxLength(500);

        builder.Property(i => i.Environment)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(SriEnvironment.Test);

        builder.Property(i => i.SriAuthorization)
            .HasMaxLength(49);

        // Relationships
        builder.HasOne(i => i.Customer)
            .WithMany()
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Warehouse)
            .WithMany()
            .HasForeignKey(i => i.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.EmissionPoint)
            .WithMany(ep => ep.Invoices)
            .HasForeignKey(i => i.EmissionPointId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Items)
            .WithOne(ii => ii.Invoice)
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(i => new { i.TenantId, i.InvoiceNumber })
            .IsUnique()
            .HasDatabaseName("IX_Invoices_TenantId_InvoiceNumber");

        builder.HasIndex(i => new { i.TenantId, i.CustomerId })
            .HasDatabaseName("IX_Invoices_TenantId_CustomerId");

        builder.HasIndex(i => new { i.TenantId, i.IssueDate })
            .HasDatabaseName("IX_Invoices_TenantId_IssueDate");

        builder.HasIndex(i => new { i.TenantId, i.Status })
            .HasDatabaseName("IX_Invoices_TenantId_Status");

        builder.HasIndex(i => i.TenantId)
            .HasDatabaseName("IX_Invoices_TenantId");

        builder.HasIndex(i => i.AccessKey)
            .IsUnique()
            .HasDatabaseName("IX_Invoices_AccessKey")
            .HasFilter("\"AccessKey\" IS NOT NULL");

        builder.HasIndex(i => i.EmissionPointId)
            .HasDatabaseName("IX_Invoices_EmissionPointId");

        // Query filter for soft delete
        builder.HasQueryFilter(i => !i.IsDeleted);
    }
}
