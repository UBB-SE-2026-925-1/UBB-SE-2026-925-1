using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Configurations;

public sealed class AmbassadorProfileConfiguration : IEntityTypeConfiguration<AmbassadorProfile>
{
    public void Configure(EntityTypeBuilder<AmbassadorProfile> builder)
    {
        builder.HasKey(ap => ap.UserId);

        builder.Property(ap => ap.PermanentCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(ap => ap.PermanentCode)
            .IsUnique();

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<AmbassadorProfile>(ap => ap.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
