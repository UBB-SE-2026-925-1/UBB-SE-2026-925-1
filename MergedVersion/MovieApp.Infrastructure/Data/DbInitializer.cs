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

        // 4.5 Seed Events
        if (!await context.Events.AnyAsync())
        {
            var movie = await context.Movies.FirstAsync();
            var events = new List<Event>
            {
                new() 
                { 
                    Id = 0,
                    Title = "The Shawshank Redemption Screening", 
                    Description = "Special screening of the classic movie.", 
                    EventDateTime = DateTime.Now.AddDays(7), 
                    LocationReference = "Main Theater, Hall 1", 
                    TicketPrice = 15.00m, 
                    EventType = "Marathon",
                    MaxCapacity = 100,
                    CreatorUserId = 1 
                },
                new() 
                { 
                    Id = 0,
                    Title = "Action Movie Night", 
                    Description = "A night of action movies.", 
                    EventDateTime = DateTime.Now.AddDays(14), 
                    LocationReference = "Cinema City, Hall 5", 
                    TicketPrice = 25.00m, 
                    EventType = "Premiere",
                    MaxCapacity = 200,
                    CreatorUserId = 2
                }
            };
            context.Events.AddRange(events);
            await context.SaveChangesAsync();
        }

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
        var categoriesToSeed = new[] { "Actors", "Directors", "Movie Quotes", "Oscars and Awards", "General Movie Trivia" };
        foreach (var category in categoriesToSeed)
        {
            if (await context.TriviaQuestions.CountAsync(q => q.Category == category) < 25)
            {
                var trivia = new List<TriviaQuestion>();
                for (int i = 1; i <= 30; i++)
                {
                    trivia.Add(new TriviaQuestion 
                    { 
                        QuestionText = $"{category} Question {i}: Sample trivia question?", 
                        Category = category, 
                        OptionA = "Option A", OptionB = "Option B", OptionC = "Option C", OptionD = "Option D", 
                        CorrectOption = 'A' 
                    });
                }
                context.TriviaQuestions.AddRange(trivia);
            }
        }
        await context.SaveChangesAsync();

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

        // 9. Seed Favorites
        var allEventsForFavs = await context.Events.ToListAsync();
        if (allEventsForFavs.Any())
        {
            if (adminUser != null && !await context.FavoriteEvents.AnyAsync(f => f.UserId == adminUser.Id))
            {
                context.FavoriteEvents.Add(new FavoriteEvent { UserId = adminUser.Id, EventId = allEventsForFavs[0].Id });
                if (allEventsForFavs.Count > 1)
                    context.FavoriteEvents.Add(new FavoriteEvent { UserId = adminUser.Id, EventId = allEventsForFavs[1].Id });
            }
            if (dummyUser != null && !await context.FavoriteEvents.AnyAsync(f => f.UserId == dummyUser.Id))
            {
                context.FavoriteEvents.Add(new FavoriteEvent { UserId = dummyUser.Id, EventId = allEventsForFavs[0].Id });
            }
            await context.SaveChangesAsync();
        }
    }
}
