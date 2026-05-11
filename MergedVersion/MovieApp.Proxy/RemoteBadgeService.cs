using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.Proxy;
public class RemoteBadgeService : IBadgeService
{
    private readonly ApiClient apiClient;
    public RemoteBadgeService(ApiClient apiClient) => this.apiClient = apiClient;
    public async Task<List<Badge>> GetUserBadgesAsync(int userId, CancellationToken ct = default) =>
        (await this.apiClient.GetAsync<IEnumerable<Badge>>($"api/users/{userId}/badges", ct))?.ToList() ?? new List<Badge>();
    public Task<List<Badge>> GetAllBadgesAsync(CancellationToken ct = default) => throw new NotImplementedException();
    public Task CheckAndAwardBadgesAsync(int userId, CancellationToken ct = default) => Task.CompletedTask; // Handled by server
}
