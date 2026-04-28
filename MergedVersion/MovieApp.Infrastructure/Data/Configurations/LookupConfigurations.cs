using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Configurations;

public sealed class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Name).IsRequired().HasMaxLength(100);

        builder.HasData(
            new Genre { Id = 1, Name = "Action" },
            new Genre { Id = 2, Name = "Comedy" },
            new Genre { Id = 3, Name = "Drama" },
            new Genre { Id = 4, Name = "Horror" },
            new Genre { Id = 5, Name = "Romance" },
            new Genre { Id = 6, Name = "Science Fiction" },
            new Genre { Id = 7, Name = "Thriller" },
            new Genre { Id = 8, Name = "Animation" },
            new Genre { Id = 9, Name = "Adventure" },
            new Genre { Id = 10, Name = "Mystery" }
        );
    }
}

public sealed class ActorConfiguration : IEntityTypeConfiguration<Actor>
{
    public void Configure(EntityTypeBuilder<Actor> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(255);

        builder.HasData(
            new Actor { Id = 1, Name = "Tom Cruise" },
            new Actor { Id = 2, Name = "Scarlett Johansson" },
            new Actor { Id = 3, Name = "Leonardo DiCaprio" },
            new Actor { Id = 4, Name = "Emma Watson" },
            new Actor { Id = 5, Name = "Dwayne Johnson" },
            new Actor { Id = 6, Name = "Jennifer Lawrence" },
            new Actor { Id = 7, Name = "Tom Hanks" },
            new Actor { Id = 8, Name = "Angelina Jolie" },
            new Actor { Id = 9, Name = "Chris Evans" },
            new Actor { Id = 10, Name = "Natalie Portman" },
            new Actor { Id = 11, Name = "Johnny Depp" },
            new Actor { Id = 12, Name = "Meryl Streep" }
        );
    }
}

public sealed class DirectorConfiguration : IEntityTypeConfiguration<Director>
{
    public void Configure(EntityTypeBuilder<Director> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(255);

        builder.HasData(
            new Director { Id = 1, Name = "Steven Spielberg" },
            new Director { Id = 2, Name = "Christopher Nolan" },
            new Director { Id = 3, Name = "Martin Scorsese" },
            new Director { Id = 4, Name = "Quentin Tarantino" },
            new Director { Id = 5, Name = "Ridley Scott" },
            new Director { Id = 6, Name = "James Cameron" },
            new Director { Id = 7, Name = "Spike Lee" },
            new Director { Id = 8, Name = "David Fincher" },
            new Director { Id = 9, Name = "Denis Villeneuve" },
            new Director { Id = 10, Name = "Wes Anderson" },
            new Director { Id = 11, Name = "Ari Aster" },
            new Director { Id = 12, Name = "Paul Thomas Anderson" }
        );
    }
}
