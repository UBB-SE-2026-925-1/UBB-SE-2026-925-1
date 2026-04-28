using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Configurations;

public sealed class MarathonConfiguration : IEntityTypeConfiguration<Marathon>
{
    public void Configure(EntityTypeBuilder<Marathon> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Title).IsRequired().HasMaxLength(255);
        
        builder.HasMany(m => m.Movies)
            .WithMany()
            .UsingEntity(j => j.ToTable("MarathonMovies"));
    }
}

public sealed class MarathonProgressConfiguration : IEntityTypeConfiguration<MarathonProgress>
{
    public void Configure(EntityTypeBuilder<MarathonProgress> builder)
    {
        builder.HasKey(mp => new { mp.UserId, mp.MarathonId });

        builder.HasOne(mp => mp.User)
            .WithMany()
            .HasForeignKey(mp => mp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Marathon>()
            .WithMany()
            .HasForeignKey(mp => mp.MarathonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
