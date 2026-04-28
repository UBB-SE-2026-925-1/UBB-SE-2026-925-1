using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Configurations;

public sealed class UserSpinDataConfiguration : IEntityTypeConfiguration<UserSpinData>
{
    public void Configure(EntityTypeBuilder<UserSpinData> builder)
    {
        builder.HasKey(usd => usd.UserId);

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<UserSpinData>(usd => usd.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed default spin data
        builder.HasData(
            new UserSpinData { UserId = 1, DailySpinsRemaining = 5, BonusSpins = 2, LoginStreak = 1, EventSpinRewardsToday = 0 },
            new UserSpinData { UserId = 2, DailySpinsRemaining = 3, BonusSpins = 1, LoginStreak = 0, EventSpinRewardsToday = 0 },
            new UserSpinData { UserId = 3, DailySpinsRemaining = 1, BonusSpins = 0, LoginStreak = 0, EventSpinRewardsToday = 0 }
        );
    }
}
