using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;

namespace MovieApp.Proxy;

public class RemoteReviewService : IReviewService
{
    private readonly ApiClient apiClient;

    public RemoteReviewService(ApiClient apiClient) => this.apiClient = apiClient;

    public async Task<List<Review>> GetReviewsForMovieAsync(int movieId, CancellationToken ct = default)
    {
        var result = await this.apiClient.GetAsync<IEnumerable<Review>>($"api/reviews/movie/{movieId}", ct);
        return result?.ToList() ?? new List<Review>();
    }

    public async Task<Review> AddReviewAsync(int userId, int movieId, float rating, string content, CancellationToken ct = default)
    {
        var result = await this.apiClient.PostAsync<object, Review>(
            "api/reviews",
            new { UserId = userId, MovieId = movieId, Rating = rating, Content = content },
            ct);
        return result ?? throw new Exception("Failed to add review.");
    }

    public async Task<double> GetAverageRatingAsync(int movieId, CancellationToken ct = default)
        => await this.apiClient.GetAsync<double>($"api/reviews/movie/{movieId}/average", ct);

    public Task DeleteReviewAsync(int reviewId, CancellationToken ct = default)
        => this.apiClient.DeleteAsync($"api/reviews/{reviewId}", ct);

    public Task UpdateReviewAsync(int reviewId, float rating, string content, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task SubmitExtraReviewAsync(int reviewId, int cgRating, string cgText, int actingRating, string actingText,
        int plotRating, string plotText, int soundRating, string soundText,
        int cinRating, string cinText, string mainExtraText, CancellationToken ct = default)
        => throw new NotImplementedException();
}
