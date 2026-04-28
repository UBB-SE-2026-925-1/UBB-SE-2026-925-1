using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.AuthProvider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.AuthSubject)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(u => new { u.AuthProvider, u.AuthSubject })
            .IsUnique();

        builder.HasOne(u => u.UserStats)
            .WithOne(s => s.User)
            .HasForeignKey<UserStats>("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        // Seed default users
        builder.HasData(
            new User
            {
                Id = 1,
                Username = "AdminUser",
                AuthProvider = "local",
                AuthSubject = "admin"
            },
            new User
            {
                Id = 2,
                Username = "UserBeta",
                AuthProvider = "local",
                AuthSubject = "beta"
            },
            new User
            {
                Id = 3,
                Username = "UserGamma",
                AuthProvider = "local",
                AuthSubject = "gamma"
            }
        );
    }
}
