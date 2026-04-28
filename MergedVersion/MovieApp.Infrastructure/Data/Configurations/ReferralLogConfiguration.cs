using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Configurations;

public sealed class ReferralLogConfiguration : IEntityTypeConfiguration<ReferralLog>
{
    public void Configure(EntityTypeBuilder<ReferralLog> builder)
    {
        builder.HasKey(rl => rl.Id);

        builder.HasOne(rl => rl.Ambassador)
            .WithMany()
            .HasForeignKey(rl => rl.AmbassadorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rl => rl.ReferredUser)
            .WithMany()
            .HasForeignKey(rl => rl.ReferredUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rl => rl.Event)
            .WithMany()
            .HasForeignKey(rl => rl.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
