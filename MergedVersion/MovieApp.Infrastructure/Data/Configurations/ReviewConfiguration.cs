using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Configurations;

public sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.ReviewId);

        builder.Property(r => r.Content)
            .HasMaxLength(5000);

        builder.Property(r => r.CinematographyText)
            .HasMaxLength(1000);

        builder.Property(r => r.ActingText)
            .HasMaxLength(1000);

        builder.Property(r => r.CgiText)
            .HasMaxLength(1000);

        builder.Property(r => r.PlotText)
            .HasMaxLength(1000);

        builder.Property(r => r.SoundText)
            .HasMaxLength(1000);

        builder.HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Movie)
            .WithMany(m => m.Reviews)
            .HasForeignKey("MovieId")
            .OnDelete(DeleteBehavior.Cascade);

        // Seed default reviews
        builder.HasData(
            new { ReviewId = 1, StarRating = 4.5f, Content = "A masterpiece of modern cinema.", IsExtraReview = true, CinematographyRating = 5, ActingRating = 5, CgiRating = 4, PlotRating = 5, SoundRating = 5, UserId = 2, MovieId = 1, CreatedAt = DateTime.UtcNow },
            new { ReviewId = 2, StarRating = 3.0f, Content = "Decent, but could be better.", IsExtraReview = false, CinematographyRating = 0, ActingRating = 0, CgiRating = 0, PlotRating = 0, SoundRating = 0, UserId = 3, MovieId = 1, CreatedAt = DateTime.UtcNow },
            new { ReviewId = 3, StarRating = 5.0f, Content = "Visually stunning and immersive.", IsExtraReview = false, CinematographyRating = 0, ActingRating = 0, CgiRating = 0, PlotRating = 0, SoundRating = 0, UserId = 2, MovieId = 5, CreatedAt = DateTime.UtcNow },
            new { ReviewId = 4, StarRating = 4.8f, Content = "The ultimate Marvel event.", IsExtraReview = false, CinematographyRating = 0, ActingRating = 0, CgiRating = 0, PlotRating = 0, SoundRating = 0, UserId = 3, MovieId = 8, CreatedAt = DateTime.UtcNow },
            new { ReviewId = 5, StarRating = 4.7f, Content = "A perfect sequel to a classic.", IsExtraReview = false, CinematographyRating = 0, ActingRating = 0, CgiRating = 0, PlotRating = 0, SoundRating = 0, UserId = 2, MovieId = 10, CreatedAt = DateTime.UtcNow },
            new { ReviewId = 6, StarRating = 4.9f, Content = "Nolan's best work yet.", IsExtraReview = false, CinematographyRating = 0, ActingRating = 0, CgiRating = 0, PlotRating = 0, SoundRating = 0, UserId = 3, MovieId = 14, CreatedAt = DateTime.UtcNow }
        );
    }
}
