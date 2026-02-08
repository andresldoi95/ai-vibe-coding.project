using SaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SaaS.Infrastructure.Persistence.Configurations;

public class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
{
    public void Configure(EntityTypeBuilder<EmailTemplate> builder)
    {
        builder.ToTable("EmailTemplates");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.BodyHtml)
            .IsRequired();

        builder.Property(e => e.BodyText);

        builder.Property(e => e.Type)
            .IsRequired();

        builder.Property(e => e.IsSystemTemplate)
            .HasDefaultValue(false);

        // Unique constraint: Name per tenant
        builder.HasIndex(e => new { e.TenantId, e.Name })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        // Indexes
        builder.HasIndex(e => new { e.TenantId, e.Type });

        // Global query filter for soft deletes
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
