using SaaS.Domain.Entities;
using SaaS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SaaS.Infrastructure.Persistence.Configurations;

public class SriConfigurationConfiguration : IEntityTypeConfiguration<SriConfiguration>
{
    public void Configure(EntityTypeBuilder<SriConfiguration> builder)
    {
        builder.ToTable("SriConfigurations");

        builder.HasKey(sc => sc.Id);

        builder.Property(sc => sc.CompanyRuc)
            .IsRequired()
            .HasMaxLength(13)
            .IsFixedLength();

        builder.Property(sc => sc.LegalName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(sc => sc.TradeName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(sc => sc.MainAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(sc => sc.AccountingRequired)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(sc => sc.Environment)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(SriEnvironment.Test);

        builder.Property(sc => sc.DigitalCertificate)
            .HasColumnType("bytea");

        builder.Property(sc => sc.CertificatePassword)
            .HasMaxLength(500); // Encrypted password can be longer

        builder.Property(sc => sc.CertificateExpiryDate);

        // Computed properties are ignored
        builder.Ignore(sc => sc.IsCertificateConfigured);
        builder.Ignore(sc => sc.IsCertificateValid);

        // Indexes
        builder.HasIndex(sc => sc.TenantId)
            .IsUnique()
            .HasDatabaseName("IX_SriConfigurations_TenantId");

        builder.HasIndex(sc => sc.CompanyRuc)
            .HasDatabaseName("IX_SriConfigurations_CompanyRuc");

        // Audit fields
        builder.Property(sc => sc.CreatedAt)
            .IsRequired();

        builder.Property(sc => sc.UpdatedAt);

        builder.Property(sc => sc.CreatedBy)
            .HasMaxLength(100);

        builder.Property(sc => sc.UpdatedBy)
            .HasMaxLength(100);
    }
}
