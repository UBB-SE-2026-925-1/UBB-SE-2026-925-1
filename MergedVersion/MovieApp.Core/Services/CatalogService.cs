using System.Threading;
using System.Threading.Tasks;
#nullable enable

using MovieApp.Core.Interfaces.Repository;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Core.Services;

/// <summary>
/// Provides logic for browsing, searching, and filtering the movie catalog.
/// </summary>
public sealed class CatalogService : ICatalogService
{
    private readonly IMovieRepository movieRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CatalogService"/> class.
    /// </summary>
    /// <param name="movieRepository">The repository for movie data.</param>
    public CatalogService(IMovieRepository movieRepository)
    {
        this.movieRepository = movieRepository;
    }

    /// <inheritdoc/>
    public async Task<List<Movie>> GetAllMoviesAsync(CancellationToken ct = default)
    {
        var movies = await this.movieRepository.GetAllAsync(ct);
        return movies.OrderBy(m => m.Title).ToList();
    }

    /// <inheritdoc/>
    public async Task<Movie?> GetMovieByIdAsync(int movieId, CancellationToken ct = default)
    {
        return await this.movieRepository.GetByIdAsync(movieId, ct);
    }

    /// <inheritdoc/>
    public async Task<List<Movie>> SearchMoviesAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await this.GetAllMoviesAsync(ct);
        }

        var movies = await this.movieRepository.GetAllAsync(ct);
        return movies
            .Where(m => m.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(m => m.Title)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<List<Movie>> FilterMoviesAsync(List<Genre> genres, float minRating, CancellationToken ct = default)
    {
        var movies = await this.movieRepository.GetAllAsync(ct);
        var filtered = movies.AsEnumerable();

        if (genres != null && genres.Count > 0)
        {
            // Logic updated for unified many-to-many Genres collection
            var genreNames = genres.Select(g => g.Name).ToList();
            filtered = filtered.Where(m => m.Genres.Any(mg => genreNames.Contains(mg.Name)));
        }

        return filtered
            .Where(m => m.AverageRating >= minRating)
            .OrderBy(m => m.Title)
            .ToList();
    }
}

