using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.UI.Services.Api;

// --- Ambassador Repository ---
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
        await this.apiClient.GetAsync<IEnumerable<ReferralHistoryItem>>($"api/referrals/user/{ambassadorId}/history", ct) ?? new List<ReferralHistoryItem>();

    public async Task<int> GetRewardBalanceAsync(int userId, CancellationToken ct = default) => 
        await this.apiClient.GetAsync<int>($"api/referrals/user/{userId}/balance", ct);

    public Task DecrementRewardBalanceAsync(int userId, CancellationToken ct = default) => 
        this.apiClient.PostAsync<object>($"api/referrals/user/{userId}/balance/decrement", new { }, ct);

    public async Task<bool> HasReferralLogAsync(int ambassadorId, int friendId, int eventId, CancellationToken ct = default) => 
        await this.apiClient.GetAsync<bool>($"api/referrals/check?ambassadorId={ambassadorId}&friendId={friendId}&eventId={eventId}", ct);
}

// --- Screening Repository ---
public class RemoteScreeningRepository : IScreeningRepository
{
    private readonly ApiClient apiClient;
    public RemoteScreeningRepository(ApiClient apiClient) => this.apiClient = apiClient;

    public async Task<IReadOnlyList<Screening>> GetByEventIdAsync(int eventIdentifier, CancellationToken ct = default) => 
        (await this.apiClient.GetAsync<IEnumerable<Screening>>($"api/screenings/event/{eventIdentifier}", ct))?.ToList().AsReadOnly() ?? new List<Screening>().AsReadOnly();

    public async Task<IReadOnlyList<Screening>> GetByMovieIdAsync(int movieIdentifier, CancellationToken ct = default) => 
        (await this.apiClient.GetAsync<IEnumerable<Screening>>($"api/screenings/movie/{movieIdentifier}", ct))?.ToList().AsReadOnly() ?? new List<Screening>().AsReadOnly();

    public Task AddAsync(Screening screening, CancellationToken ct = default) => throw new NotImplementedException();
}

// --- UserMovieDiscount Repository ---
public class RemoteUserMovieDiscountRepository : IUserMovieDiscountRepository
{
    private readonly ApiClient apiClient;
    public RemoteUserMovieDiscountRepository(ApiClient apiClient) => this.apiClient = apiClient;

    public async Task<List<Reward>> GetDiscountsForUserAsync(int userId, CancellationToken ct = default) => 
        (await this.apiClient.GetAsync<IEnumerable<Reward>>($"api/rewards/user/{userId}", ct))?.ToList() ?? new List<Reward>();

    public Task MarkRedeemedAsync(int rewardId, CancellationToken ct = default) => 
        this.apiClient.PostAsync<object>($"api/rewards/{rewardId}/redeem", new { }, ct);

    public Task AddAsync(Reward reward, CancellationToken ct = default) => throw new NotImplementedException();
}

// --- UserEventAttendance Repository ---
public class RemoteUserEventAttendanceRepository : IUserEventAttendanceRepository
{
    private readonly ApiClient apiClient;
    public RemoteUserEventAttendanceRepository(ApiClient apiClient) => this.apiClient = apiClient;

    public async Task<IReadOnlyList<int>> GetJoinedEventIdsAsync(int userIdentifier, CancellationToken ct = default) => 
        (await this.apiClient.GetAsync<IEnumerable<int>>($"api/events/user/{userIdentifier}/attendance", ct))?.ToList().AsReadOnly() ?? new List<int>().AsReadOnly();

    public Task JoinAsync(int userIdentifier, int eventIdentifier, CancellationToken ct = default) => 
        this.apiClient.PostAsync<object>($"api/events/user/{userIdentifier}/attendance/join?eventId={eventIdentifier}", new { }, ct);
}
