using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence.Configurations;

public class UserInvitationConfiguration : IEntityTypeConfiguration<UserInvitation>
{
    public void Configure(EntityTypeBuilder<UserInvitation> builder)
    {
        builder.ToTable("UserInvitations");

        builder.HasKey(ui => ui.Id);

        builder.HasIndex(ui => ui.InvitationToken)
            .IsUnique();

        builder.Property(ui => ui.InvitationToken)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ui => ui.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(ui => ui.CreatedAt)
            .IsRequired();

        builder.Property(ui => ui.ExpiresAt)
            .IsRequired();

        builder.Property(ui => ui.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(ui => ui.Tenant)
            .WithMany()
            .HasForeignKey(ui => ui.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ui => ui.Role)
            .WithMany()
            .HasForeignKey(ui => ui.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ui => ui.InvitedByUser)
            .WithMany()
            .HasForeignKey(ui => ui.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
