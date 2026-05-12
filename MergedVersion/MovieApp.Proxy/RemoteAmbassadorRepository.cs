using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.Proxy;
public class RemoteAmbassadorRepository : IAmbassadorRepository
{
    private readonly ApiClient apiClient;
    public RemoteAmbassadorRepository(ApiClient apiClient) => this.apiClient = apiClient;

    public async Task<bool> IsReferralCodeValidAsync(string referralCode, CancellationToken ct = default) =>
        await this.apiClient.GetAsync<bool>($"api/referrals/validate?code={referralCode}", ct);

    public async Task<string?> GetReferralCodeAsync(int userId, CancellationToken ct = default) =>
        await this.apiClient.GetAsync<string>($"api/referrals/user/{userId}/code", ct);

    public Task CreateAmbassadorProfileAsync(int userId, string referralCode, CancellationToken ct = default) =>
        this.apiClient.PostAsync<object>($"api/referrals/profile", new { UserId = userId, Code = referralCode }, ct);

    public async Task<int?> GetUserIdByReferralCodeAsync(string referralCode, CancellationToken ct = default) =>
        await this.apiClient.GetAsync<int?>($"api/referrals/code/{referralCode}/user", ct);

    public Task AddReferralLogAsync(int ambassadorId, int friendId, int eventId, CancellationToken ct = default) =>
        this.apiClient.PostAsync<object>($"api/referrals/log", new { AmbassadorId = ambassadorId, FriendId = friendId, EventId = eventId }, ct);

    public async Task<bool> TryApplyRewardAsync(int ambassadorId, CancellationToken ct = default) =>
        await this.apiClient.PostAsync<object, bool>($"api/referrals/reward/apply", new { AmbassadorId = ambassadorId }, ct);

    public async Task<IEnumerable<ReferralHistoryItem>> GetReferralHistoryAsync(int ambassadorId, CancellationToken ct = default) =>
        await this.apiClient.GetAsync<List<ReferralHistoryItem>>($"api/referrals/user/{ambassadorId}/history", ct) ?? new List<ReferralHistoryItem>();

    public async Task<int> GetRewardBalanceAsync(int userId, CancellationToken ct = default) =>
        await this.apiClient.GetAsync<int>($"api/referrals/user/{userId}/balance", ct);

    public Task DecrementRewardBalanceAsync(int userId, CancellationToken ct = default) =>
        this.apiClient.PostAsync<object>($"api/referrals/user/{userId}/balance/decrement", new { }, ct);

    public async Task<bool> HasReferralLogAsync(int ambassadorId, int friendId, int eventId, CancellationToken ct = default) =>
        await this.apiClient.GetAsync<bool>($"api/referrals/check?ambassadorId={ambassadorId}&friendId={friendId}&eventId={eventId}", ct);
}
