using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.Proxy;

public class RemoteRewardService : IRewardService
{
    private readonly ApiClient apiClient;

    public RemoteRewardService(ApiClient apiClient)
    {
        this.apiClient = apiClient;
    }

    public async Task<IReadOnlyList<Reward>> GetRewardsForUserAsync(
        int userIdentifier,
        CancellationToken cancellationToken = default)
    {
        return (await this.apiClient.GetAsync<IEnumerable<Reward>>(
                    $"api/rewards/user/{userIdentifier}",
                    cancellationToken))
               ?.ToList().AsReadOnly()
               ?? new List<Reward>().AsReadOnly();
    }

    public async Task<bool> RedeemAsync(
        Reward reward,
        int? eventIdentifier,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await this.apiClient.PostAsync<object>(
                $"api/rewards/{reward.RewardId}/redeem",
                new { },
                cancellationToken);

            return true;
        }
        catch
        {
            return false;
        }
    }
}