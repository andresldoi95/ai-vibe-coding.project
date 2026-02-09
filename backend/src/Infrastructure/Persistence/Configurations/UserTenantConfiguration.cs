using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Configurations;

public class UserTenantConfiguration : IEntityTypeConfiguration<UserTenant>
{
    public void Configure(EntityTypeBuilder<UserTenant> builder)
    {
        builder.ToTable("UserTenants");

        builder.HasKey(ut => ut.Id);

        builder.HasIndex(ut => new { ut.UserId, ut.TenantId })
            .IsUnique();

        builder.HasOne(ut => ut.User)
            .WithMany(u => u.UserTenants)
            .HasForeignKey(ut => ut.UserId);

        builder.HasOne(ut => ut.Tenant)
            .WithMany(t => t.UserTenants)
            .HasForeignKey(ut => ut.TenantId);

        builder.HasOne(ut => ut.Role)
            .WithMany(r => r.UserTenants)
            .HasForeignKey(ut => ut.RoleId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false); // Nullable during migration
    }
}
