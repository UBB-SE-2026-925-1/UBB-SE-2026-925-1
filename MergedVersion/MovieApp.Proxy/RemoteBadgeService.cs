using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.WebAPI.Controllers.DTOs;

namespace MovieApp.Proxy;

public class RemoteBadgeService : IBadgeService
{
    private readonly ApiClient apiClient;

    public RemoteBadgeService(ApiClient apiClient) => this.apiClient = apiClient;

    public async Task<UserBadgesDTO> GetUserBadgesAsync(int userId, CancellationToken ct = default)
        => await this.apiClient.GetAsync<UserBadgesDTO>($"api/users/{userId}/badges", ct)
        ?? new UserBadgesDTO { UserId = userId };

    public async Task<List<Badge>> GetAllBadgesAsync(CancellationToken ct = default)
        => await this.apiClient.GetAsync<List<Badge>>("api/badges", ct) ?? new List<Badge>();

    public Task CheckAndAwardBadgesAsync(int userId, CancellationToken ct = default)
        => this.apiClient.PostAsync($"api/users/{userId}/badges/check", new { }, ct);
}
