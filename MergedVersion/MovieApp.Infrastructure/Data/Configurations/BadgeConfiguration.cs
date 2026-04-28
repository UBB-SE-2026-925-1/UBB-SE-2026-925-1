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

        // Seed default badges
        builder.HasData(
            new Badge { BadgeId = 1, Name = "Film Fanatic", CriteriaValue = 10 },
            new Badge { BadgeId = 2, Name = "Reviewer", CriteriaValue = 5 },
            new Badge { BadgeId = 3, Name = "Social Butterfly", CriteriaValue = 20 },
            new Badge { BadgeId = 4, Name = "Early Bird", CriteriaValue = 1 },
            new Badge { BadgeId = 5, Name = "Movie Buff", CriteriaValue = 50 },
            new Badge { BadgeId = 6, Name = "Elite Member", CriteriaValue = 100 }
        );
    }
}
