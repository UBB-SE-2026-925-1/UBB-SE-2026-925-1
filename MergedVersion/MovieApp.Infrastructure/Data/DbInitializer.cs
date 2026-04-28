using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieApp.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(MovieAppDbContext context)
    {
        context.Database.EnsureCreated();

        // Smart seeding will handle duplicates/missing data below

        // 1. Seed Genres
        var genres = new List<Genre>
        {
            new() { Name = "Action" },
            new() { Name = "Comedy" },
            new() { Name = "Drama" },
            new() { Name = "Horror" },
            new() { Name = "Romance" },
            new() { Name = "Science Fiction" },
            new() { Name = "Thriller" },
            new() { Name = "Animation" },
            new() { Name = "Adventure" },
            new() { Name = "Mystery" }
        };
        if (!await context.Genres.AnyAsync())
        {
            context.Genres.AddRange(genres);
            await context.SaveChangesAsync();
        }

        // 2. Seed Actors
        var actors = new List<Actor>
        {
            new() { Name = "Tom Cruise" },
            new() { Name = "Scarlett Johansson" },
            new() { Name = "Leonardo DiCaprio" },
            new() { Name = "Emma Watson" },
            new() { Name = "Dwayne Johnson" },
            new() { Name = "Jennifer Lawrence" },
            new() { Name = "Tom Hanks" },
            new() { Name = "Angelina Jolie" },
            new() { Name = "Chris Evans" },
            new() { Name = "Natalie Portman" },
            new() { Name = "Johnny Depp" },
            new() { Name = "Meryl Streep" }
        };
        if (!await context.Actors.AnyAsync())
        {
            context.Actors.AddRange(actors);
            await context.SaveChangesAsync();
        }

        // 3. Seed Directors
        var directors = new List<Director>
        {
            new() { Name = "Steven Spielberg" },
            new() { Name = "Christopher Nolan" },
            new() { Name = "Martin Scorsese" },
            new() { Name = "Quentin Tarantino" },
            new() { Name = "Ridley Scott" },
            new() { Name = "James Cameron" },
            new() { Name = "Spike Lee" },
            new() { Name = "David Fincher" },
            new() { Name = "Denis Villeneuve" },
            new() { Name = "Wes Anderson" },
            new() { Name = "Ari Aster" }
        };
        if (!await context.Directors.AnyAsync())
        {
            context.Directors.AddRange(directors);
            await context.SaveChangesAsync();
        }

        await context.SaveChangesAsync();

        // 4. Seed Movies
        var moviesToSeed = new List<Movie>
        {
            new() { Title = "The Shawshank Redemption", Description = "Two imprisoned men bond over a number of years.", ReleaseYear = 1994, DurationMinutes = 142, AverageRating = 4.5, PosterUrl = "https://images.remote.com/poster1.jpg" },
            new() { Title = "The Dark Knight", Description = "When the menace known as the Joker wreaks havoc.", ReleaseYear = 2008, DurationMinutes = 152, AverageRating = 4.3, PosterUrl = "https://images.remote.com/poster2.jpg" },
            new() { Title = "Inception", Description = "A thief who steals corporate secrets through the use of dream-sharing technology.", ReleaseYear = 2010, DurationMinutes = 148, AverageRating = 4.2, PosterUrl = "https://images.remote.com/poster3.jpg" },
            new() { Title = "Pulp Fiction", Description = "The lives of two mob hitmen, a boxer, a gangster and his wife.", ReleaseYear = 1994, DurationMinutes = 154, AverageRating = 4.4, PosterUrl = "https://images.remote.com/poster4.jpg" },
            new() { Title = "Interstellar", Description = "A team of explorers travel through a wormhole in space.", ReleaseYear = 2014, DurationMinutes = 169, AverageRating = 4.6, PosterUrl = "https://images.remote.com/poster5.jpg" },
            new() { Title = "The Avengers", Description = "Earth's mightiest heroes must come together.", ReleaseYear = 2012, DurationMinutes = 143, AverageRating = 4.1, PosterUrl = "https://images.remote.com/poster6.jpg" },
            new() { Title = "Mission Impossible", Description = "An American agent, under false suspicion of disloyalty.", ReleaseYear = 1996, DurationMinutes = 110, AverageRating = 3.9, PosterUrl = "https://images.remote.com/poster7.jpg" }
        };

        foreach (var m in moviesToSeed)
        {
            if (!await context.Movies.AnyAsync(existing => existing.Title == m.Title))
            {
                context.Movies.Add(m);
            }
        }
        await context.SaveChangesAsync();

        // 5. Seed Badges (Team B)
        if (!await context.Badges.AnyAsync())
        {
            var badges = new List<Badge>
            {
                new() { Name = "The Snob", CriteriaValue = 10 },
                new() { Name = "Why so serious?", CriteriaValue = 50 },
                new() { Name = "The Joker", CriteriaValue = 70 },
                new() { Name = "The Godfather I", CriteriaValue = 100 },
                new() { Name = "The Godfather II", CriteriaValue = 200 },
                new() { Name = "The Godfather III", CriteriaValue = 300 }
            };
            context.Badges.AddRange(badges);
        }

        // 6. Seed Trivia (Team A Sample)
        if (!await context.TriviaQuestions.AnyAsync())
        {
            var trivia = new List<TriviaQuestion>
            {
                new() { QuestionText = "Which actor played Iron Man in the Marvel Cinematic Universe?", Category = "Actors", OptionA = "Chris Evans", OptionB = "Robert Downey Jr.", OptionC = "Mark Ruffalo", OptionD = "Chris Hemsworth", CorrectOption = 'B' },
                new() { QuestionText = "Who directed Inception (2010)?", Category = "Directors", OptionA = "Steven Spielberg", OptionB = "Christopher Nolan", OptionC = "James Cameron", OptionD = "Ridley Scott", CorrectOption = 'B' },
                new() { QuestionText = "Which film contains the quote: \"Here is looking at you, kid\"?", Category = "Movie Quotes", OptionA = "Gone with the Wind", OptionB = "Casablanca", OptionC = "Sunset Boulevard", OptionD = "Rebecca", CorrectOption = 'B' }
            };
            context.TriviaQuestions.AddRange(trivia);
        }

        // 7. Seed Users
        User? adminUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "Admin");
        User? dummyUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "Dummy User");
        
        bool usersAdded = false;
        if (adminUser == null)
        {
            adminUser = new User { Username = "Admin", AuthProvider = "Seed", AuthSubject = "Admin" };
            context.Users.Add(adminUser);
            usersAdded = true;
        }
        if (dummyUser == null)
        {
            dummyUser = new User { Username = "Dummy User", AuthProvider = "dummy", AuthSubject = "default-user" };
            context.Users.Add(dummyUser);
            usersAdded = true;
        }

        if (usersAdded || context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }

        // 8. Seed UserStats
        if (!await context.UserStats.AnyAsync())
        {
            if (adminUser != null && !await context.UserStats.AnyAsync(us => us.User != null && us.User.Id == adminUser.Id))
                context.UserStats.Add(new UserStats { User = adminUser, TotalPoints = 1000, WeeklyScore = 500 });
            
            if (dummyUser != null && !await context.UserStats.AnyAsync(us => us.User != null && us.User.Id == dummyUser.Id))
                context.UserStats.Add(new UserStats { User = dummyUser, TotalPoints = 50, WeeklyScore = 10 });

            await context.SaveChangesAsync();
        }
    }
}
