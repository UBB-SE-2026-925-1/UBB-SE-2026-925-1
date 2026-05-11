using System;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;

namespace MovieApp.Proxy;

public class RemoteBattleService : IBattleService
{
    private readonly ApiClient apiClient;

    public RemoteBattleService(ApiClient apiClient)
    {
        this.apiClient = apiClient;
    }

    public async Task<Battle?> GetActiveBattleAsync(CancellationToken ct = default)
    {
        return await this.apiClient.GetAsync<Battle>("api/battles/active", ct);
    }

    public async Task<Bet> PlaceBetAsync(int userId, int battleId, int movieId, int amount, CancellationToken ct = default)
    {
        var request = new { UserId = userId, BattleId = battleId, MovieId = movieId, Amount = amount };
        var result = await this.apiClient.PostAsync<object, Bet>("api/battles/bet", request, ct);
        return result ?? throw new Exception("Failed to place bet");
    }

    public async Task<Bet?> GetBetAsync(int userId, int battleId, CancellationToken ct = default)
    {
        return await this.apiClient.GetAsync<Bet>($"api/battles/user/{userId}/bet/{battleId}", ct);
    }

    public async Task ResetAllBattlesForDemoAsync(CancellationToken ct = default)
    {
        await this.apiClient.PostAsync<object>("api/battles/reset", new { }, ct);
    }

    public async Task<Battle> CreateDemoBattleAsync(CancellationToken ct = default)
    {
        // On the server, Reset already creates a new demo battle
        return await GetActiveBattleAsync(ct) ?? throw new Exception("Failed to create demo battle");
    }

    // These methods are administrative and might not be used by the standard UI client, 
    // but we implement them to satisfy the interface.
    public Task<Battle> CreateBattleAsync(int firstMovieId, int secondMovieId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<int> DetermineWinnerAsync(int battleId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task DistributePayoutsAsync(int battleId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task SettleExpiredBattlesAsync(CancellationToken ct = default) => throw new NotImplementedException();
    public Task<Battle?> GetCurrentBattleForUserAsync(int userId, CancellationToken ct = default) => GetActiveBattleAsync(ct);
    public Task ForceSettleBattleAsync(int battleId, CancellationToken ct = default) => throw new NotImplementedException();
}
