using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Configurations;

public sealed class UserEventAttendanceConfiguration : IEntityTypeConfiguration<UserEventAttendance>
{
    public void Configure(EntityTypeBuilder<UserEventAttendance> builder)
    {
        builder.HasKey(ua => new { ua.UserId, ua.EventId });

        builder.HasOne(ua => ua.User)
            .WithMany()
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ua => ua.Event)
            .WithMany()
            .HasForeignKey(ua => ua.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
