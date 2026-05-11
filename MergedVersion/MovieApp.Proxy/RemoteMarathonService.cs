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

    public Task<MarathonProgress?> GetCurrentProgressAsync(int marathonIdentifier) => throw new NotImplementedException();
    public Task UpdateQuizResultAsync(int marathonIdentifier, int correctAnswersCount) => throw new NotImplementedException();
    public Task<bool> LogMovieAsync(int marathonIdentifier, int movieIdentifier, int correctAnswersCount) => throw new NotImplementedException();
    public Task<int> GetParticipantCountAsync(int marathonId) => throw new NotImplementedException();
    public Task<int> GetMarathonMovieCountAsync(int marathonId) => throw new NotImplementedException();
    public Task<bool> IsPrerequisiteCompletedAsync(int userId, int marathonId) => throw new NotImplementedException();
    public Task<IEnumerable<LeaderboardEntry>> GetLeaderboardAsync(int marathonId) => throw new NotImplementedException();
}
