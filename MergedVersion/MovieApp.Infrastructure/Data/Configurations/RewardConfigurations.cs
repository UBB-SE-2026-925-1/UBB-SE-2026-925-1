using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Configurations;

public sealed class RewardConfiguration : IEntityTypeConfiguration<Reward>
{
    public void Configure(EntityTypeBuilder<Reward> builder)
    {
        builder.HasKey(r => r.RewardId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(r => r.OwnerUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class TriviaRewardConfiguration : IEntityTypeConfiguration<TriviaReward>
{
    public void Configure(EntityTypeBuilder<TriviaReward> builder)
    {
        builder.HasKey(tr => tr.Id);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(tr => tr.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
