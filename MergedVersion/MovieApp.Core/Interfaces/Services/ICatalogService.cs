using System.Threading;
using System.Threading.Tasks;
#nullable enable

using MovieApp.Core.Models;
using MovieApp.Core.Models;

namespace MovieApp.Core.Interfaces.Service;

/// <summary>
/// Defines business logic for browsing and filtering the movie catalog.
/// </summary>
public interface ICatalogService
{
    /// <summary>Retrieves every movie available in the catalog.</summary>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    Task<List<Movie>> GetAllMoviesAsync(CancellationToken ct = default);

    /// <summary>Retrieves detailed metadata for a single movie.</summary>
    /// <param name="movieId">The identifier of the movie.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    Task<Movie?> GetMovieByIdAsync(int movieId, CancellationToken ct = default);

    /// <summary>Finds movies whose titles contain the specified query string.</summary>
    /// <param name="query">The search term.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    Task<List<Movie>> SearchMoviesAsync(string query, CancellationToken ct = default);

    /// <summary>Filters the catalog based on genre memberships and rating thresholds.</summary>
    /// <param name="genres">The list of genres to filter by.</param>
    /// <param name="minRating">The minimum average rating required.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    Task<List<Movie>> FilterMoviesAsync(List<Genre> genres, float minRating, CancellationToken ct = default);
}

