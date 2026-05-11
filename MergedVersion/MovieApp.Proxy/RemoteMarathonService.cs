using MovieApp.Core.Models;
using MovieApp.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.Proxy;
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
        this.GetUserProgressAsync(IdentityConfig.CurrentUserId, marathonIdentifier);
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