using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

using MovieApp.Core.Services;

namespace MovieApp.UI.Services.Api;

public class RemoteEventRepository : IEventRepository
{
    private readonly ApiClient apiClient;
    public RemoteEventRepository(ApiClient apiClient) => this.apiClient = apiClient;

    public async Task<IEnumerable<Event>> GetAllAsync(CancellationToken ct = default) => 
        await this.apiClient.GetAsync<IEnumerable<Event>>("api/events", ct) ?? new List<Event>();

    public async Task<Event?> FindByIdAsync(int eventId, CancellationToken ct = default) => 
        await this.apiClient.GetAsync<Event>($"api/events/{eventId}", ct);

    public async Task<IEnumerable<Event>> GetAllByTypeAsync(string eventType, CancellationToken ct = default) => 
        (await GetAllAsync(ct)).Where(e => e.EventType == eventType);

    // Write operations (if needed by UI)
    public Task<int> AddAsync(Event eventDetails, CancellationToken ct = default) => this.apiClient.PostAsync<Event, int>("api/events", eventDetails, ct);
    public Task<bool> UpdateAsync(Event eventDetails, CancellationToken ct = default) => throw new NotImplementedException(); // Replaced by UpdateEventAsync below
    public async Task<bool> UpdateEnrollmentAsync(int eventId, int newEnrollmentCount, CancellationToken ct = default) => 
        await this.apiClient.PostAsync<object, bool>($"api/events/{eventId}/enrollment?count={newEnrollmentCount}", new { }, ct);
    public Task UpdateEventAsync(Event updatedEvent, CancellationToken ct = default) => 
        this.apiClient.PostAsync<Event>("api/events/update", updatedEvent, ct);
    public async Task<bool> DeleteAsync(int eventId, CancellationToken ct = default)
    {
        await this.apiClient.DeleteAsync($"api/events/{eventId}", ct);
        return true;
    }
}

public class RemoteFavoriteEventService : IFavoriteEventService
{
    private readonly ApiClient apiClient;
    public RemoteFavoriteEventService(ApiClient apiClient) => this.apiClient = apiClient;

    public Task AddFavoriteAsync(int userIdentifier, int eventIdentifier, CancellationToken ct = default) => 
        this.apiClient.PostAsync<object>($"api/events/favorites/toggle", new { UserId = userIdentifier, EventId = eventIdentifier }, ct);

    public Task RemoveFavoriteAsync(int userIdentifier, int eventIdentifier, CancellationToken ct = default) => 
        this.apiClient.PostAsync<object>($"api/events/favorites/toggle", new { UserId = userIdentifier, EventId = eventIdentifier }, ct);

    public async Task<IReadOnlyList<FavoriteEvent>> GetFavoritesByUserAsync(int userIdentifier, CancellationToken ct = default) => 
        (await this.apiClient.GetAsync<IEnumerable<FavoriteEvent>>($"api/events/favorites/{userIdentifier}", ct))?.ToList().AsReadOnly() ?? new List<FavoriteEvent>().AsReadOnly();

    public async Task<bool> ExistsFavoriteAsync(int userIdentifier, int eventIdentifier, CancellationToken ct = default) => 
        await this.apiClient.GetAsync<bool>($"api/events/favorites/check?userId={userIdentifier}&eventId={eventIdentifier}", ct);

    public async Task<IReadOnlyList<Event>> GetFavoriteEventsByUserIdAsync(int userIdentifier, CancellationToken ct = default) => 
        (await this.apiClient.GetAsync<IEnumerable<Event>>($"api/events/favorites/details/{userIdentifier}", ct))?.ToList().AsReadOnly() ?? new List<Event>().AsReadOnly();
}
