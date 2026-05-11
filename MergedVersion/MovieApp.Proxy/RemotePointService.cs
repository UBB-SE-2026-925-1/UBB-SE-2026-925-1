using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;

namespace MovieApp.Proxy;

public class RemotePointService : IPointService
{
    private readonly ApiClient apiClient;

    public RemotePointService(ApiClient apiClient) => this.apiClient = apiClient;

    public async Task<UserStats> GetUserStatsAsync(int userId, CancellationToken ct = default)
    {
        var stats = await this.apiClient.GetAsync<UserStats>($"api/users/{userId}/stats", ct);
        return stats ?? throw new InvalidOperationException($"No stats found for user {userId}.");
    }

    public Task AddPointsAsync(int userId, int movieId, bool isBattleMovie, CancellationToken ct = default)
        => this.apiClient.PostAsync($"api/users/{userId}/points/add", new { movieId, isBattleMovie }, ct);

    public Task DeductPointsAsync(int userId, int points, CancellationToken ct = default)
        => this.apiClient.PostAsync($"api/users/{userId}/points/deduct", new { points }, ct);

    public Task FreezePointsAsync(int userId, int amount, CancellationToken ct = default)
        => this.apiClient.PostAsync($"api/users/{userId}/points/freeze", new { amount }, ct);

    public Task RefundPointsAsync(int userId, int amount, CancellationToken ct = default)
        => this.apiClient.PostAsync($"api/users/{userId}/points/refund", new { amount }, ct);

    public Task UpdateWeeklyScoreAsync(int userId, CancellationToken ct = default)
        => this.apiClient.PostAsync($"api/users/{userId}/points/weekly-score", new { }, ct);
}
