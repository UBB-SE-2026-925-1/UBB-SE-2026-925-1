using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Configurations;

public sealed class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        // 1. Primary Key
        builder.HasKey(m => m.Id);

        // 2. Property Constraints
        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(m => m.Description)
            .HasMaxLength(2000);

        builder.Property(m => m.PosterUrl)
            .HasMaxLength(500);

        // 4. One-to-Many Relationships
        // Ensuring that when a movie is deleted, its reviews and comments are handled

        builder.HasMany(m => m.Reviews)
            .WithOne(r => r.Movie)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Comments)
            .WithOne(c => c.Movie)
            .OnDelete(DeleteBehavior.Cascade);

        // 5. Indexing
        builder.HasIndex(m => m.Title);

        // 6. Seed Data
        builder.HasData(
            new Movie { Id = 1, Title = "The Shawshank Redemption", ReleaseYear = 1994, Description = "Two imprisoned men bond over a number of years, finding solace and eventual redemption through acts of common decency.", PosterUrl = "https://image.tmdb.org/t/p/w500/20f2GThu22hp5MgCA4dg3bZ3gTS.jpg", AverageRating = 3.8 },
            new Movie { Id = 2, Title = "The Dark Knight", ReleaseYear = 2008, Description = "When the menace known as the Joker wreaks havoc and chaos on the people of Gotham, Batman must accept one of the greatest psychological and physical tests of his ability to fight injustice.", PosterUrl = "https://image.tmdb.org/t/p/w500/qJ2tW6WMUDux911r6m7haRef0WH.jpg" },
            new Movie { Id = 3, Title = "Inception", ReleaseYear = 2010, Description = "A thief who steals corporate secrets through the use of dream-sharing technology is given the inverse task of planting an idea into the mind of a C.E.O.", PosterUrl = "https://image.tmdb.org/t/p/w500/ljsZTbVsrQSqZgWeep2B1QiDKuh.jpg" },
            new Movie { Id = 4, Title = "Pulp Fiction", ReleaseYear = 1994, Description = "The lives of two mob hitmen, a boxer, a gangster and his wife, and a pair of diner bandits intertwine in four tales of violence and redemption.", PosterUrl = "https://image.tmdb.org/t/p/w500/d5iIlFn5s0ImszYzBPb8JPIfbXD.jpg" },
            new Movie { Id = 5, Title = "Avatar", ReleaseYear = 2009, Description = "A paraplegic Marine dispatched to the moon Pandora on a unique mission becomes torn between following his orders and protecting the world he feels is his home.", PosterUrl = "https://image.tmdb.org/t/p/w500/kyeqWdyUXW608qlYkRqosgbbJyK.jpg", AverageRating = 5.0 },
            new Movie { Id = 6, Title = "Interstellar", ReleaseYear = 2014, Description = "A team of explorers travel through a wormhole in space in an attempt to ensure humanity's survival.", PosterUrl = "https://image.tmdb.org/t/p/w500/yQvGrMoipbRoddT0ZR8tPoR7NfX.jpg" },
            new Movie { Id = 7, Title = "Mission: Impossible - Fallout", ReleaseYear = 2018, Description = "Ethan Hunt and his IMF team, along with some familiar allies, race against time after a mission goes wrong.", PosterUrl = "https://image.tmdb.org/t/p/w500/AkJQpZp9WoNdj7pLYSj1L0RcMMN.jpg" },
            new Movie { Id = 8, Title = "Avengers: Infinity War", ReleaseYear = 2018, Description = "The Avengers and their allies must be willing to sacrifice all in an attempt to defeat the powerful Thanos before his blitz of devastation and ruin puts an end to the universe.", PosterUrl = "https://image.tmdb.org/t/p/w500/7WsyChQLEftFiDOVTGkv3hFpyyt.jpg", AverageRating = 4.8 },
            new Movie { Id = 9, Title = "The Wolf of Wall Street", ReleaseYear = 2013, Description = "Based on the true story of Jordan Belfort, from his rise to a wealthy stock-broker living the high life to his fall involving crime, corruption and the federal government.", PosterUrl = "https://image.tmdb.org/t/p/w500/kW9LmvYHAaS9iA0tHmZVq8hQYoq.jpg" },
            new Movie { Id = 10, Title = "Blade Runner 2049", ReleaseYear = 2017, Description = "Young Blade Runner K's discovery of a long-buried secret leads him to track down former Blade Runner Rick Deckard, who's been missing for thirty years.", PosterUrl = "https://image.tmdb.org/t/p/w500/gajva2L0rPYkEWjzgFlBXCAVBE5.jpg", AverageRating = 4.7 },
            new Movie { Id = 11, Title = "Forrest Gump", ReleaseYear = 1994, Description = "The presidencies of Kennedy and Johnson, the Vietnam War, the Watergate scandal and other historical events unfold from the perspective of an Alabama man with an IQ of 75.", PosterUrl = "https://image.tmdb.org/t/p/w500/Cw4hIUIAmSYfK9QfaUW5igp9La.jpg" },
            new Movie { Id = 12, Title = "Gladiator", ReleaseYear = 2000, Description = "A former Roman General sets out to exact vengeance against the corrupt emperor who murdered his family and sent him into slavery.", PosterUrl = "https://image.tmdb.org/t/p/w500/wN2xWp1eIwCKOD0BHTcErTBv1Uq.jpg" },
            new Movie { Id = 13, Title = "The Matrix", ReleaseYear = 1999, Description = "A computer hacker learns from mysterious rebels about the true nature of his reality and his role in the war against its controllers.", PosterUrl = "https://image.tmdb.org/t/p/w500/dXNAPwY7VrqMAo51EKhhCJfaGb5.jpg" },
            new Movie { Id = 14, Title = "Dunkirk", ReleaseYear = 2017, Description = "Allied soldiers from Belgium, the British Empire and France are surrounded by the German Army, and evacuated during a fierce battle in World War II.", PosterUrl = "https://image.tmdb.org/t/p/w500/ebSnODDg9lbsMIaWg2uAbjn7TO5.jpg", AverageRating = 4.9 }
        );

        // Seed many-to-many relationships
        builder.HasMany(m => m.Genres)
            .WithMany()
            .UsingEntity(
                "MovieGenres",
                l => l.HasOne(typeof(Genre)).WithMany().HasForeignKey("GenresId"),
                r => r.HasOne(typeof(Movie)).WithMany().HasForeignKey("MoviesId"),
                j =>
                {
                    j.HasKey("MoviesId", "GenresId");
                    j.HasData(
                        new { MoviesId = 1, GenresId = 3 }, // Shawshank - Drama
                        new { MoviesId = 2, GenresId = 1 }, // Dark Knight - Action
                        new { MoviesId = 2, GenresId = 7 }, // Dark Knight - Thriller
                        new { MoviesId = 3, GenresId = 6 }, // Inception - SciFi
                        new { MoviesId = 4, GenresId = 7 }, // Pulp Fiction - Thriller
                        new { MoviesId = 5, GenresId = 1 }, // Avatar - Action
                        new { MoviesId = 5, GenresId = 6 }, // Avatar - SciFi
                        new { MoviesId = 6, GenresId = 6 }, // Interstellar - SciFi
                        new { MoviesId = 7, GenresId = 1 }, // Mission Impossible - Action
                        new { MoviesId = 8, GenresId = 1 }, // Avengers - Action
                        new { MoviesId = 9, GenresId = 3 }, // Wolf of WS - Drama
                        new { MoviesId = 10, GenresId = 6 }, // Blade Runner - SciFi
                        new { MoviesId = 11, GenresId = 3 }, // Forrest Gump - Drama
                        new { MoviesId = 12, GenresId = 1 }, // Gladiator - Action
                        new { MoviesId = 12, GenresId = 3 }, // Gladiator - Drama
                        new { MoviesId = 13, GenresId = 6 }, // Matrix - SciFi
                        new { MoviesId = 14, GenresId = 3 }  // Dunkirk - Drama
                    );
                });

        // 7. Seed Reviews (to populate AverageRating)
        builder.HasMany(m => m.Reviews)
            .WithOne(r => r.Movie)
            .HasForeignKey("MovieId") // Ensure FK is correct
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Actors)
            .WithMany()
            .UsingEntity(
                "MovieActors",
                l => l.HasOne(typeof(Actor)).WithMany().HasForeignKey("ActorsId"),
                r => r.HasOne(typeof(Movie)).WithMany().HasForeignKey("MoviesId"),
                j =>
                {
                    j.HasKey("MoviesId", "ActorsId");
                    j.HasData(
                        new { MoviesId = 3, ActorsId = 3 }, // Inception - DiCaprio
                        new { MoviesId = 6, ActorsId = 3 }, // Interstellar - DiCaprio
                        new { MoviesId = 7, ActorsId = 1 }, // MI - Cruise
                        new { MoviesId = 8, ActorsId = 2 }, // Avengers - Scarlett
                        new { MoviesId = 11, ActorsId = 4 } // Forrest Gump - Emma Watson (Mock data link)
                    );
                });

        builder.HasMany(m => m.Directors)
            .WithMany()
            .UsingEntity(
                "MovieDirectors",
                l => l.HasOne(typeof(Director)).WithMany().HasForeignKey("DirectorsId"),
                r => r.HasOne(typeof(Movie)).WithMany().HasForeignKey("MoviesId"),
                j =>
                {
                    j.HasKey("MoviesId", "DirectorsId");
                    j.HasData(
                        new { MoviesId = 2, DirectorsId = 2 }, // Dark Knight - Nolan
                        new { MoviesId = 3, DirectorsId = 2 }, // Inception - Nolan
                        new { MoviesId = 6, DirectorsId = 2 }, // Interstellar - Nolan
                        new { MoviesId = 14, DirectorsId = 2 }, // Dunkirk - Nolan
                        new { MoviesId = 9, DirectorsId = 3 },  // Wolf of WS - Scorsese
                        new { MoviesId = 4, DirectorsId = 4 }   // Pulp Fiction - Tarantino
                    );
                });
    }
}
