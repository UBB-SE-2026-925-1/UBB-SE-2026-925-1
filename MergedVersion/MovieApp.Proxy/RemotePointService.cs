using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;

namespace MovieApp.Proxy;

public class RemotePointService : IPointService
{
    private readonly ApiClient apiClient;

    public RemotePointService(ApiClient apiClient) => this.apiClient = apiClient;

    public async Task<UserStats> GetUserStatsAsync(int userId, CancellationToken ct = default)
        => await this.apiClient.GetAsync<UserStats>($"api/users/{userId}/stats", ct)
        ?? throw new InvalidOperationException("The API did not return user stats.");

    public Task AddPointsAsync(int userId, int movieId, bool isBattleMovie, CancellationToken ct = default)
        => throw new NotSupportedException("Point mutations are handled by the Web API.");

    public Task DeductPointsAsync(int userId, int points, CancellationToken ct = default)
        => throw new NotSupportedException("Point mutations are handled by the Web API.");

    public Task FreezePointsAsync(int userId, int amount, CancellationToken ct = default)
        => throw new NotSupportedException("Point mutations are handled by the Web API.");

    public Task RefundPointsAsync(int userId, int amount, CancellationToken ct = default)
        => throw new NotSupportedException("Point mutations are handled by the Web API.");

    public Task UpdateWeeklyScoreAsync(int userId, CancellationToken ct = default)
        => throw new NotSupportedException("Point mutations are handled by the Web API.");
}
