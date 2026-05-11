using System.Globalization;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;

namespace MovieApp.Proxy;

public class RemoteCatalogService : ICatalogService
{
    private readonly ApiClient apiClient;

    public RemoteCatalogService(ApiClient apiClient) => this.apiClient = apiClient;

    public async Task<List<Movie>> GetAllMoviesAsync(CancellationToken ct = default)
    {
        var result = await this.apiClient.GetAsync<IEnumerable<Movie>>("api/movies", ct);
        return result?.ToList() ?? new List<Movie>();
    }

    public async Task<Movie?> GetMovieByIdAsync(int movieId, CancellationToken ct = default)
        => await this.apiClient.GetAsync<Movie>($"api/movies/{movieId}", ct);

    public async Task<List<Movie>> SearchMoviesAsync(string query, CancellationToken ct = default)
    {
        var result = await this.apiClient.GetAsync<IEnumerable<Movie>>($"api/movies?query={Uri.EscapeDataString(query)}", ct);
        return result?.ToList() ?? new List<Movie>();
    }

    public async Task<List<Movie>> FilterMoviesAsync(List<Genre> genres, float minRating, CancellationToken ct = default)
    {
        // URL-encode each genre name and use invariant culture for the decimal separator
        var genreQuery = string.Join(",", genres.Select(g => Uri.EscapeDataString(g.Name)));
        var ratingStr = minRating.ToString(CultureInfo.InvariantCulture);
        var result = await this.apiClient.GetAsync<IEnumerable<Movie>>($"api/movies?genres={genreQuery}&minRating={ratingStr}", ct);
        return result?.ToList() ?? new List<Movie>();
    }
}
