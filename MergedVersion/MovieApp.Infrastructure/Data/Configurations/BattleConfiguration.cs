using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Configurations;

public sealed class BattleConfiguration : IEntityTypeConfiguration<Battle>
{
    public void Configure(EntityTypeBuilder<Battle> builder)
    {
        builder.HasKey(b => b.BattleId);

        builder.Property(b => b.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasOne(b => b.FirstMovie)
            .WithMany()
            .HasForeignKey("FirstMovieId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.SecondMovie)
            .WithMany()
            .HasForeignKey("SecondMovieId")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
