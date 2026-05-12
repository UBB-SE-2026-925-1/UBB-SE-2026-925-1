using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.Proxy;
public class RemoteFavoriteEventService : IFavoriteEventService
{
    private readonly ApiClient apiClient;
    public RemoteFavoriteEventService(ApiClient apiClient) => this.apiClient = apiClient;

    public Task AddFavoriteAsync(int userIdentifier, int eventIdentifier, CancellationToken ct = default) =>
        this.apiClient.PostAsync<object>($"api/events/favorites/toggle", new { UserId = userIdentifier, EventId = eventIdentifier }, ct);

    public Task RemoveFavoriteAsync(int userIdentifier, int eventIdentifier, CancellationToken ct = default) =>
        this.apiClient.PostAsync<object>($"api/events/favorites/toggle", new { UserId = userIdentifier, EventId = eventIdentifier }, ct);

    public async Task<IReadOnlyList<FavoriteEvent>> GetFavoritesByUserAsync(int userIdentifier, CancellationToken ct = default) =>
        (await this.apiClient.GetAsync<List<FavoriteEvent>>($"api/events/favorites/{userIdentifier}", ct))?.AsReadOnly() ?? new List<FavoriteEvent>().AsReadOnly();

    public async Task<bool> ExistsFavoriteAsync(int userIdentifier, int eventIdentifier, CancellationToken ct = default) =>
        await this.apiClient.GetAsync<bool>($"api/events/favorites/check?userId={userIdentifier}&eventId={eventIdentifier}", ct);

    public async Task<IReadOnlyList<Event>> GetFavoriteEventsByUserIdAsync(int userIdentifier, CancellationToken ct = default) =>
        (await this.apiClient.GetAsync<List<Event>>($"api/events/favorites/details/{userIdentifier}", ct))?.AsReadOnly() ?? new List<Event>().AsReadOnly();
}
