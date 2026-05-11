using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;

namespace MovieApp.Proxy;

public class RemoteBattleService : IBattleService
{
    private readonly ApiClient apiClient;

    public RemoteBattleService(ApiClient apiClient) => this.apiClient = apiClient;

    public async Task<Battle?> GetActiveBattleAsync(CancellationToken ct = default)
    {
        try
        {
            return await this.apiClient.GetAsync<Battle>("api/battles/active", ct);
        }
        catch
        {
            return null;
        }
    }

    public async Task<Battle> CreateBattleAsync(int firstMovieId, int secondMovieId, CancellationToken ct = default)
        => await this.apiClient.PostAsync<CreateBattleRequest, Battle>(
            "api/battles",
            new CreateBattleRequest(firstMovieId, secondMovieId),
            ct)
        ?? throw new InvalidOperationException("The API did not return the created battle.");

    public async Task<Bet> PlaceBetAsync(int userId, int battleId, int movieId, int amount, CancellationToken ct = default)
        => await this.apiClient.PostAsync<PlaceBetRequest, Bet>(
            "api/battles/bet",
            new PlaceBetRequest(userId, battleId, movieId, amount),
            ct)
        ?? throw new InvalidOperationException("The API did not return the placed bet.");

    public async Task<Bet?> GetBetAsync(int userId, int battleId, CancellationToken ct = default)
    {
        try
        {
            return await this.apiClient.GetAsync<Bet>($"api/battles/user/{userId}/bet/{battleId}", ct);
        }
        catch
        {
            return null;
        }
    }

    public async Task<int> DetermineWinnerAsync(int battleId, CancellationToken ct = default)
        => await this.apiClient.GetAsync<int>($"api/battles/{battleId}/winner", ct);

    public Task DistributePayoutsAsync(int battleId, CancellationToken ct = default)
        => this.ForceSettleBattleAsync(battleId, ct);

    public Task SettleExpiredBattlesAsync(CancellationToken ct = default)
        => this.apiClient.PostAsync("api/battles/settle-expired", new { }, ct);

    public async Task<Battle?> GetCurrentBattleForUserAsync(int userId, CancellationToken ct = default)
    {
        try
        {
            return await this.apiClient.GetAsync<Battle>($"api/battles/user/{userId}/current", ct);
        }
        catch
        {
            return null;
        }
    }

    public Task ForceSettleBattleAsync(int battleId, CancellationToken ct = default)
        => this.apiClient.PostAsync($"api/battles/{battleId}/settle", new { }, ct);

    public Task ResetAllBattlesForDemoAsync(CancellationToken ct = default)
        => this.apiClient.PostAsync("api/battles/reset", new { }, ct);

    public async Task<Battle> CreateDemoBattleAsync(CancellationToken ct = default)
        => await this.apiClient.PostAsync<object, Battle>("api/battles/demo", new { }, ct)
        ?? throw new InvalidOperationException("The API did not return the demo battle.");

    private sealed record PlaceBetRequest(int UserId, int BattleId, int MovieId, int Amount);

    private sealed record CreateBattleRequest(int FirstMovieId, int SecondMovieId);
}
