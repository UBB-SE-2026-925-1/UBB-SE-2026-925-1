using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Configurations;

public sealed class BadgeConfiguration : IEntityTypeConfiguration<Badge>
{
    public void Configure(EntityTypeBuilder<Badge> builder)
    {
        builder.HasKey(b => b.BadgeId);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.Description)
            .IsRequired()
            .HasMaxLength(300)
            .HasDefaultValue(string.Empty);

        // Seed default badges
        builder.HasData(
            new Badge { BadgeId = 1, Name = "Film Fanatic", Description = "Reach 10 profile progress points.", CriteriaValue = 10 },
            new Badge { BadgeId = 2, Name = "Reviewer", Description = "Reach 5 profile progress points.", CriteriaValue = 5 },
            new Badge { BadgeId = 3, Name = "Social Butterfly", Description = "Reach 20 profile progress points.", CriteriaValue = 20 },
            new Badge { BadgeId = 4, Name = "Early Bird", Description = "Reach 1 profile progress point.", CriteriaValue = 1 },
            new Badge { BadgeId = 5, Name = "Movie Buff", Description = "Reach 50 profile progress points.", CriteriaValue = 50 },
            new Badge { BadgeId = 6, Name = "Elite Member", Description = "Reach 100 profile progress points.", CriteriaValue = 100 }
        );
    }
}
