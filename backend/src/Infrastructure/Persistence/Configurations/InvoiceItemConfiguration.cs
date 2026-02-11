using SaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SaaS.Infrastructure.Persistence.Configurations;

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("InvoiceItems");

        builder.Property(ii => ii.InvoiceId)
            .IsRequired();

        builder.Property(ii => ii.ProductId)
            .IsRequired();

        // Denormalized product fields
        builder.Property(ii => ii.ProductCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ii => ii.ProductName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(ii => ii.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(ii => ii.Quantity)
            .IsRequired();

        builder.Property(ii => ii.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(ii => ii.TaxRateId)
            .IsRequired();

        builder.Property(ii => ii.TaxRate)
            .IsRequired()
            .HasPrecision(5, 4);

        builder.Property(ii => ii.SubtotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(ii => ii.TaxAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(ii => ii.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        // Relationships
        builder.HasOne(ii => ii.Invoice)
            .WithMany(i => i.Items)
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ii => ii.Product)
            .WithMany()
            .HasForeignKey(ii => ii.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ii => ii.TaxRateEntity)
            .WithMany()
            .HasForeignKey(ii => ii.TaxRateId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(ii => ii.InvoiceId)
            .HasDatabaseName("IX_InvoiceItems_InvoiceId");

        builder.HasIndex(ii => ii.ProductId)
            .HasDatabaseName("IX_InvoiceItems_ProductId");
    }
}
