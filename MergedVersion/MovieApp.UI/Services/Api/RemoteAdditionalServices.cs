using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.UI.Services.Api;

// --- Comment Service ---
public class RemoteCommentService : ICommentService
{
    private readonly ApiClient apiClient;
    public RemoteCommentService(ApiClient apiClient) => this.apiClient = apiClient;
    public async Task<List<Comment>> GetCommentsForMovieAsync(int movieId, CancellationToken ct = default) => 
        (await this.apiClient.GetAsync<IEnumerable<Comment>>($"api/comments/movie/{movieId}", ct))?.ToList() ?? new List<Comment>();
    public async Task<Comment> AddCommentAsync(int userId, int movieId, string content, CancellationToken ct = default) =>
        await this.apiClient.PostAsync<object, Comment>("api/comments", new { UserId = userId, MovieId = movieId, Content = content }, ct) ?? throw new Exception();
    public async Task<Comment> AddReplyAsync(int userId, int parentCommentId, string content, CancellationToken ct = default) =>
        await this.apiClient.PostAsync<object, Comment>("api/comments/reply", new { UserId = userId, ParentCommentId = parentCommentId, Content = content }, ct) ?? throw new Exception();
    public Task DeleteCommentAsync(int commentId, CancellationToken ct = default) => this.apiClient.DeleteAsync($"api/comments/{commentId}", ct);
}

// --- Badge Service ---
public class RemoteBadgeService : IBadgeService
{
    private readonly ApiClient apiClient;
    public RemoteBadgeService(ApiClient apiClient) => this.apiClient = apiClient;
    public async Task<List<Badge>> GetUserBadgesAsync(int userId, CancellationToken ct = default) => 
        (await this.apiClient.GetAsync<IEnumerable<Badge>>($"api/users/{userId}/badges", ct))?.ToList() ?? new List<Badge>();
    public Task<List<Badge>> GetAllBadgesAsync(CancellationToken ct = default) => throw new NotImplementedException();
    public Task CheckAndAwardBadgesAsync(int userId, CancellationToken ct = default) => Task.CompletedTask; // Handled by server
}

// --- Point Service ---
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

// --- Marathon Service ---
public class RemoteMarathonService : IMarathonService
{
    private readonly ApiClient apiClient;
    public RemoteMarathonService(ApiClient apiClient) => this.apiClient = apiClient;
    public async Task<IEnumerable<Marathon>> GetWeeklyMarathonsAsync(int userIdentifier) => 
        await this.apiClient.GetAsync<IEnumerable<Marathon>>($"api/marathons/weekly/{userIdentifier}") ?? new List<Marathon>();
    public async Task<IEnumerable<Movie>> GetMoviesForMarathonAsync(int marathonId) => 
        await this.apiClient.GetAsync<IEnumerable<Movie>>($"api/marathons/{marathonId}/movies") ?? new List<Movie>();
    public async Task<MarathonProgress?> GetUserProgressAsync(int userId, int marathonId) => 
        await this.apiClient.GetAsync<MarathonProgress>($"api/marathons/{marathonId}/progress/{userId}");
    public async Task<bool> StartMarathonAsync(int marathonIdentifier) => 
        await this.apiClient.PostAsync<object, bool>($"api/marathons/{marathonIdentifier}/start", new { });
    public async Task<IEnumerable<LeaderboardEntry>> GetLeaderboardWithUsernamesAsync(int marathonId) => 
        await this.apiClient.GetAsync<IEnumerable<LeaderboardEntry>>($"api/marathons/{marathonId}/leaderboard") ?? new List<LeaderboardEntry>();
    
    public Task<MarathonProgress?> GetCurrentProgressAsync(int marathonIdentifier) => 
        this.GetUserProgressAsync(App.CurrentUserId, marathonIdentifier);
    public async Task UpdateQuizResultAsync(int marathonIdentifier, int correctAnswersCount) => 
        await this.apiClient.PostAsync<object>($"api/marathons/{marathonIdentifier}/quiz", correctAnswersCount);
    public async Task<bool> LogMovieAsync(int marathonIdentifier, int movieIdentifier, int correctAnswersCount) => 
        await this.apiClient.PostAsync<int, bool>($"api/marathons/{marathonIdentifier}/movies/{movieIdentifier}/log", correctAnswersCount);
    public async Task<int> GetParticipantCountAsync(int marathonId) => 
        await this.apiClient.GetAsync<int>($"api/marathons/{marathonId}/participants/count");
    public async Task<int> GetMarathonMovieCountAsync(int marathonId) => 
        await this.apiClient.GetAsync<int>($"api/marathons/{marathonId}/movies/count");
    public async Task<bool> IsPrerequisiteCompletedAsync(int userId, int marathonId) => 
        await this.apiClient.GetAsync<bool>($"api/marathons/{marathonId}/prerequisite/{userId}");
    public Task<IEnumerable<LeaderboardEntry>> GetLeaderboardAsync(int marathonId) => 
        this.GetLeaderboardWithUsernamesAsync(marathonId);
}

// --- Notification Service ---
public class RemoteNotificationService : INotificationService
{
    private readonly ApiClient apiClient;
    public RemoteNotificationService(ApiClient apiClient) => this.apiClient = apiClient;
    public async Task<IReadOnlyList<Notification>> GetNotificationsByUserIdAsync(int userIdentifier, CancellationToken ct = default) => 
        (await this.apiClient.GetAsync<IEnumerable<Notification>>($"api/notifications/user/{userIdentifier}", ct))?.ToList().AsReadOnly() ?? new List<Notification>().AsReadOnly();
    public Task RemoveNotificationAsync(int notificationIdentifier, CancellationToken ct = default) => this.apiClient.DeleteAsync($"api/notifications/{notificationIdentifier}", ct);
    public Task MarkAsReadOrRemoveAsync(int notificationIdentifier, CancellationToken ct = default) => this.apiClient.PostAsync<object>($"api/notifications/{notificationIdentifier}/read", new { }, ct);
    
    public Task GeneratePriceDropNotificationAsync(int eventIdentifier, string eventTitle, CancellationToken ct = default) => throw new NotImplementedException();
    public Task GenerateSeatsAvailableNotificationAsync(int eventIdentifier, string eventTitle, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<IReadOnlyList<Notification>> GetNotificationsByUserAsync(int userIdentifier, CancellationToken ct = default) => GetNotificationsByUserIdAsync(userIdentifier, ct);
    public Task NotifyPriceDropAsync(int eventIdentifier, decimal oldPrice, decimal newPrice, CancellationToken ct = default) => throw new NotImplementedException();
    public Task NotifySeatsAvailableAsync(int eventIdentifier, int newCapacity, CancellationToken ct = default) => throw new NotImplementedException();
}

// --- Slot Machine Service ---
public class RemoteSlotMachineService : ISlotMachineService
{
    private readonly ApiClient apiClient;
    public RemoteSlotMachineService(ApiClient apiClient) => this.apiClient = apiClient;
    public async Task<SlotMachineResult> SpinAsync(int userIdentifier) => 
        await this.apiClient.PostAsync<object, SlotMachineResult>($"api/slotmachine/spin/{userIdentifier}", new { }) ?? throw new Exception();
    public async Task<UserSpinData> GetUserSpinStateAsync(int userIdentifier) => 
        await this.apiClient.GetAsync<UserSpinData>($"api/slotmachine/state/{userIdentifier}") ?? throw new Exception();
    public async Task<IReadOnlyList<Genre>> GetGenresAsync(CancellationToken ct = default) => 
        (await this.apiClient.GetAsync<IEnumerable<Genre>>("api/slotmachine/reels/genres", ct))?.ToList().AsReadOnly() ?? new List<Genre>().AsReadOnly();
    public async Task<IReadOnlyList<Actor>> GetActorsAsync(CancellationToken ct = default) => 
        (await this.apiClient.GetAsync<IEnumerable<Actor>>("api/slotmachine/reels/actors", ct))?.ToList().AsReadOnly() ?? new List<Actor>().AsReadOnly();
    public async Task<IReadOnlyList<Director>> GetDirectorsAsync(CancellationToken ct = default) => 
        (await this.apiClient.GetAsync<IEnumerable<Director>>("api/slotmachine/reels/directors", ct))?.ToList().AsReadOnly() ?? new List<Director>().AsReadOnly();

    public async Task<int> GetAvailableSpinsAsync(int userIdentifier) => 
        await this.apiClient.GetAsync<int>($"api/slotmachine/available/{userIdentifier}");
    public async Task<bool> GrantBonusSpinForEventParticipationAsync(int userIdentifier) => 
        await this.apiClient.PostAsync<object, bool>($"api/slotmachine/bonus/{userIdentifier}", new { });
    public async Task<bool> RecordLoginAndCheckStreakAsync(int userIdentifier) => 
        await this.apiClient.PostAsync<object, bool>($"api/slotmachine/login-streak/{userIdentifier}", new { });
    public async Task<bool> GrantStreakSpinAsync(int userIdentifier) => 
        await this.apiClient.PostAsync<object, bool>($"api/slotmachine/streak-spin/{userIdentifier}", new { });
    public async Task<Genre> GetRandomGenreAsync(CancellationToken ct = default) => 
        await this.apiClient.GetAsync<Genre>("api/slotmachine/reels/genres/random", ct) ?? throw new Exception();
    public async Task<Actor> GetRandomActorAsync(CancellationToken ct = default) => 
        await this.apiClient.GetAsync<Actor>("api/slotmachine/reels/actors/random", ct) ?? throw new Exception();
    public async Task<Director> GetRandomDirectorAsync(CancellationToken ct = default) => 
        await this.apiClient.GetAsync<Director>("api/slotmachine/reels/directors/random", ct) ?? throw new Exception();
    public Task<IReadOnlyList<Event>> GetMatchingEventsAsync(int genreIdentifier, int actorIdentifier, int directorIdentifier) => throw new NotImplementedException();
    public Task<Movie?> FindJackpotMovieAsync(int genreIdentifier, int actorIdentifier, int directorIdentifier) => throw new NotImplementedException();
    public Task GrantJackpotDiscount(int userIdentifier, int movieIdentifier) => throw new NotImplementedException();
}
