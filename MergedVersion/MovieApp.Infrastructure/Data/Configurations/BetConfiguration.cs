using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Configurations;

public sealed class BetConfiguration : IEntityTypeConfiguration<Bet>
{
    public void Configure(EntityTypeBuilder<Bet> builder)
    {
        // Composite PK
        builder.HasKey("UserId", "BattleId");

        builder.HasOne(b => b.User)
            .WithMany(u => u.Bets)
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Battle)
            .WithMany(ba => ba.Bets)
            .HasForeignKey("BattleId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Movie)
            .WithMany()
            .HasForeignKey("MovieId")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
