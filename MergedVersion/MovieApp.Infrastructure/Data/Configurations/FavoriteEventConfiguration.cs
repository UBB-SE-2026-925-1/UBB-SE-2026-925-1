using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Configurations;

public sealed class FavoriteEventConfiguration : IEntityTypeConfiguration<FavoriteEvent>
{
    public void Configure(EntityTypeBuilder<FavoriteEvent> builder)
    {
        builder.HasKey(fe => fe.Id);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(fe => fe.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Event>()
            .WithMany()
            .HasForeignKey(fe => fe.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
