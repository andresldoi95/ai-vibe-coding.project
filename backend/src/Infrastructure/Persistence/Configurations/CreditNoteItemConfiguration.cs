using SaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SaaS.Infrastructure.Persistence.Configurations;

public class CreditNoteItemConfiguration : IEntityTypeConfiguration<CreditNoteItem>
{
    public void Configure(EntityTypeBuilder<CreditNoteItem> builder)
    {
        builder.ToTable("CreditNoteItems");

        builder.Property(ci => ci.CreditNoteId)
            .IsRequired();

        builder.Property(ci => ci.ProductId)
            .IsRequired();

        // Denormalized product fields
        builder.Property(ci => ci.ProductCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ci => ci.ProductName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(ci => ci.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(ci => ci.Quantity)
            .IsRequired();

        builder.Property(ci => ci.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(ci => ci.TaxRateId)
            .IsRequired();

        builder.Property(ci => ci.TaxRate)
            .IsRequired()
            .HasPrecision(5, 4);

        builder.Property(ci => ci.SubtotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(ci => ci.TaxAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(ci => ci.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        // Relationships
        builder.HasOne(ci => ci.CreditNote)
            .WithMany(cn => cn.Items)
            .HasForeignKey(ci => ci.CreditNoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ci => ci.Product)
            .WithMany()
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ci => ci.TaxRateEntity)
            .WithMany()
            .HasForeignKey(ci => ci.TaxRateId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(ci => ci.CreditNoteId)
            .HasDatabaseName("IX_CreditNoteItems_CreditNoteId");

        builder.HasIndex(ci => ci.ProductId)
            .HasDatabaseName("IX_CreditNoteItems_ProductId");
    }
}
