using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Proxy;
public class RemoteUserMovieDiscountRepository : IUserMovieDiscountRepository
{
    private readonly ApiClient apiClient;
    public RemoteUserMovieDiscountRepository(ApiClient apiClient) => this.apiClient = apiClient;

    public async Task<List<Reward>> GetDiscountsForUserAsync(int userId, CancellationToken ct = default) =>
        (await this.apiClient.GetAsync<List<Reward>>($"api/rewards/user/{userId}", ct)) ?? new List<Reward>();

    public Task MarkRedeemedAsync(int rewardId, CancellationToken ct = default) =>
        this.apiClient.PostAsync<object>($"api/rewards/{rewardId}/redeem", new { }, ct);

    public Task AddAsync(Reward reward, CancellationToken ct = default) =>
        this.apiClient.PostAsync<Reward>("api/rewards", reward, ct);
}
