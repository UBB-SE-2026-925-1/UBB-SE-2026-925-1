using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.Proxy;
public class RemotePointService : IPointService
{
    private readonly ApiClient apiClient;
    public RemotePointService(ApiClient apiClient) => this.apiClient = apiClient;
    public async Task<UserStats> GetUserStatsAsync(int userId, CancellationToken ct = default) =>
        await this.apiClient.GetAsync<UserStats>($"api/users/{userId}/stats", ct) ?? throw new Exception();
    public Task AddPointsAsync(int userId, int movieId, bool isBattleMovie, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeductPointsAsync(int userId, int points, CancellationToken ct = default) => Task.CompletedTask;
    public Task FreezePointsAsync(int userId, int amount, CancellationToken ct = default) => Task.CompletedTask;
    public Task RefundPointsAsync(int userId, int amount, CancellationToken ct = default) => Task.CompletedTask;
    public Task UpdateWeeklyScoreAsync(int userId, CancellationToken ct = default) => Task.CompletedTask;
}