using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Configurations;

public sealed class UserStatsConfiguration : IEntityTypeConfiguration<UserStats>
{
    public void Configure(EntityTypeBuilder<UserStats> builder)
    {
        builder.HasKey(s => s.StatsId);

        // One-to-one is handled in UserConfiguration, 
        // but we can ensure the FK property name matches here if needed.

        builder.HasData(
            new { StatsId = 1, TotalPoints = 1500, WeeklyScore = 200, UserId = 1 },
            new { StatsId = 2, TotalPoints = 450, WeeklyScore = 50, UserId = 2 },
            new { StatsId = 3, TotalPoints = 100, WeeklyScore = 10, UserId = 3 }
        );
    }
}
