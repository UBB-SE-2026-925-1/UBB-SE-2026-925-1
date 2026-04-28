using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Configurations;

public sealed class ScreeningConfiguration : IEntityTypeConfiguration<Screening>
{
    public void Configure(EntityTypeBuilder<Screening> builder)
    {
        builder.HasKey(s => s.Id);

        builder.HasOne<Event>()
            .WithMany()
            .HasForeignKey(s => s.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<MovieApp.Core.Models.Movie>()
            .WithMany()
            .HasForeignKey(s => s.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed default screenings
        builder.HasData(
            new Screening { Id = 1, MovieId = 1, EventId = 1, ScreeningTime = DateTime.Now.AddDays(1) },
            new Screening { Id = 2, MovieId = 2, EventId = 1, ScreeningTime = DateTime.Now.AddDays(2) },
            new Screening { Id = 3, MovieId = 3, EventId = 2, ScreeningTime = DateTime.Now.AddDays(3) }
        );
    }
}
