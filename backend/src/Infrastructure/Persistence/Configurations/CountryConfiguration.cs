using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Country entity
/// </summary>
public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Alpha3Code)
            .HasMaxLength(3);

        builder.Property(c => c.NumericCode)
            .HasMaxLength(3);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Unique constraint on Code
        builder.HasIndex(c => c.Code)
            .IsUnique()
            .HasDatabaseName("IX_Countries_Code");

        // Index on Name for sorting/searching
        builder.HasIndex(c => c.Name)
            .HasDatabaseName("IX_Countries_Name");

        // Index on IsActive for filtering
        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("IX_Countries_IsActive");
    }
}
