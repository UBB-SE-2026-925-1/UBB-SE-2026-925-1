using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Configurations;

public sealed class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.LocationReference)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.EventType)
            .HasMaxLength(50);

        builder.Property(e => e.TicketPrice)
            .HasPrecision(18, 2);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.CreatorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed default events
        builder.HasData(
            new Event
            {
                Id = 1,
                Title = "Cannes Winner Screening",
                Description = "Special screening of the Palme d'Or winner.",
                EventDateTime = new DateTime(2026, 5, 15, 19, 0, 0),
                LocationReference = "Cinema Hall A",
                TicketPrice = 25.00m,
                EventType = "Premiere",
                HistoricalRating = 4.8,
                MaxCapacity = 100,
                CurrentEnrollment = 45,
                CreatorUserId = 1
            },
            new Event
            {
                Id = 2,
                Title = "Vintage Film Marathon",
                Description = "Back-to-back classics from the 50s.",
                EventDateTime = new DateTime(2026, 5, 20, 10, 0, 0),
                LocationReference = "Retro Cinema",
                TicketPrice = 40.00m,
                EventType = "Marathon",
                HistoricalRating = 4.5,
                MaxCapacity = 50,
                CurrentEnrollment = 10,
                CreatorUserId = 1
            },
            new Event
            {
                Id = 3,
                Title = "Director's Q&A: Sci-Fi Night",
                Description = "Watch the latest sci-fi hit followed by a talk.",
                EventDateTime = new DateTime(2026, 5, 25, 20, 0, 0),
                LocationReference = "Tech Hub Theater",
                TicketPrice = 15.00m,
                EventType = "Special",
                HistoricalRating = 4.9,
                MaxCapacity = 200,
                CurrentEnrollment = 150,
                CreatorUserId = 1
            }
        );
    }
}
