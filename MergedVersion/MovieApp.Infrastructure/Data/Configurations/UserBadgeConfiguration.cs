using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Configurations;

public sealed class UserBadgeConfiguration : IEntityTypeConfiguration<UserBadge>
{
    public void Configure(EntityTypeBuilder<UserBadge> builder)
    {
        // Composite PK
        builder.HasKey("UserId", "BadgeId");

        builder.HasOne(ub => ub.User)
            .WithMany(u => u.UserBadges)
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ub => ub.Badge)
            .WithMany(b => b.UserBadges)
            .HasForeignKey("BadgeId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
