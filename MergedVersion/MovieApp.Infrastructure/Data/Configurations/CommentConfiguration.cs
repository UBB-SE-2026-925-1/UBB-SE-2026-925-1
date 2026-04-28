using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Configurations;

public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.MessageId);

        builder.Property(c => c.Content)
            .IsRequired()
            .HasMaxLength(10000);

        builder.HasOne(c => c.Author)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Movie)
            .WithMany(m => m.Comments) // Corrected from m.Reviews
            .HasForeignKey(c => c.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.NoAction);

        // Seed default forum comments
        builder.HasData(
            new Comment { MessageId = 1, AuthorId = 2, MovieId = 1, Content = "I can't wait for the sequel!", CreatedAt = DateTime.UtcNow.AddHours(-5) },
            new Comment { MessageId = 2, AuthorId = 3, MovieId = 1, Content = "Agreed, the ending was insane.", ParentCommentId = 1, CreatedAt = DateTime.UtcNow.AddHours(-2) }
        );
    }
}
