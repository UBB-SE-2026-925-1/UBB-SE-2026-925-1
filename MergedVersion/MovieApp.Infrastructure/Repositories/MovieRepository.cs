using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models; 
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure.Repositories;

public sealed class MovieRepository : IMovieRepository
{
    private readonly MovieAppDbContext context;

    public MovieRepository(MovieAppDbContext context) => this.context = context;

    public async Task<IReadOnlyList<Genre>> GetGenresAsync(CancellationToken ct = default)
        => await this.context.Genres.ToListAsync(ct);

    public async Task<IReadOnlyList<Actor>> GetActorsAsync(CancellationToken ct = default)
        => await this.context.Actors.ToListAsync(ct);

    public async Task<IReadOnlyList<Director>> GetDirectorsAsync(CancellationToken ct = default)
        => await this.context.Directors.ToListAsync(ct);

    public async Task<IReadOnlyList<Movie>> FindMoviesByCriteriaAsync(int genreId, int actorId, int directorId, CancellationToken ct = default)
    {
        return await this.context.Movies
            .Where(m => m.Genres.Any(g => g.Id == genreId) &&
                        m.Actors.Any(a => a.Id == actorId) &&
                        m.Directors.Any(d => d.Id == directorId))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Movie>> FindMoviesByAnyCriteriaAsync(int genreId, int actorId, int directorId, CancellationToken ct = default)
    {
        return await this.context.Movies
            .Where(m => m.Genres.Any(g => g.Id == genreId) ||
                        m.Actors.Any(a => a.Id == actorId) ||
                        m.Directors.Any(d => d.Id == directorId))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<int>> FindScreeningEventIdsForMovieAsync(int movieId, CancellationToken ct = default)
    {
        return await this.context.Screenings
            .Where(s => s.MovieId == movieId)
            .Select(s => s.EventId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ReelCombination>> GetValidReelCombinationsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        // Get movies that have at least one screening in the future
        var moviesWithFutureScreenings = await this.context.Movies
            .Include(m => m.Genres)
            .Include(m => m.Actors)
            .Include(m => m.Directors)
            .Where(m => this.context.Screenings.Any(s => s.MovieId == m.Id && s.ScreeningTime >= now))
            .ToListAsync(ct);

        var combinations = new List<ReelCombination>();

        foreach (var movie in moviesWithFutureScreenings)
        {
            foreach (var genre in movie.Genres)
            {
                foreach (var actor in movie.Actors)
                {
                    foreach (var director in movie.Directors)
                    {
                        combinations.Add(new ReelCombination
                        {
                            Genre = genre,
                            Actor = actor,
                            Director = director
                        });
                    }
                }
            }
        }

        // Filter to unique combinations based on the IDs of the entities
        return combinations
            .DistinctBy(c => new
            {
                GenreId = c.Genre.Id,
                ActorId = c.Actor.Id,
                DirectorId = c.Director.Id
            })
            .ToList();
    }

    public async Task<List<Movie>> GetAllAsync(CancellationToken ct = default)
    {
        var movies = await this.context.Movies
            .Include(m => m.Genres)
            .Include(m => m.Actors)
            .Include(m => m.Directors)
            .ToListAsync(ct);
        
        movies = movies
            .GroupBy(m => m.Title)
            .Select(g => g.OrderByDescending(m => !string.IsNullOrEmpty(m.PosterUrl)).First())
            .ToList();
        
        foreach (var movie in movies)
        {
            this.InitializeMovie(movie);
        }

        return movies;
    }

    public async Task<Movie?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var movie = await this.context.Movies
            .Include(m => m.Genres)
            .Include(m => m.Actors)
            .Include(m => m.Directors)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (movie != null)
        {
            this.InitializeMovie(movie);
        }

        return movie;
    }

    private void InitializeMovie (Movie movie)
    {
        movie.Genres ??= new List<Genre>();
        movie.Actors ??= new List<Actor>();
        movie.Directors ??= new List<Director>();
        movie.Title ??= "Untitled Movie";
        movie.PosterUrl ??= string.Empty;
        movie.Description ??= string.Empty;
    }

    public async Task<int> InsertAsync(Movie movie, CancellationToken ct = default)
    {
        this.context.Movies.Add(movie);
        await this.context.SaveChangesAsync(ct);
        return movie.Id;
    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken ct = default)
    {
        this.context.Movies.Update(movie);
        return await this.context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var movie = await this.context.Movies.FindAsync(new object[] { id }, ct);
        if (movie == null) return false;
        this.context.Movies.Remove(movie);
        return await this.context.SaveChangesAsync(ct) > 0;
    }
}


