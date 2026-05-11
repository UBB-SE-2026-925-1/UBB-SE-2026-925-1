using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Proxy;

public class RemoteEventRepository : IEventRepository
{
    private readonly ApiClient apiClient;

    public RemoteEventRepository(ApiClient apiClient) => this.apiClient = apiClient;

    public async Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var result = await this.apiClient.GetAsync<IEnumerable<Event>>("api/events", cancellationToken);
        return result ?? new List<Event>();
    }

    public async Task<IEnumerable<Event>> GetAllByTypeAsync(string eventType, CancellationToken cancellationToken = default)
    {
        var all = await this.GetAllAsync(cancellationToken);
        return all.Where(e => string.Equals(e.EventType, eventType, System.StringComparison.OrdinalIgnoreCase));
    }

    public Task<Event?> FindByIdAsync(int eventId, CancellationToken cancellationToken = default) =>
        this.apiClient.GetAsync<Event>($"api/events/{eventId}", cancellationToken);

    public async Task<int> AddAsync(Event eventDetails, CancellationToken cancellationToken = default) =>
        await this.apiClient.PostAsync<Event, int>("api/events", eventDetails, cancellationToken);

    public async Task<bool> UpdateAsync(Event eventDetails, CancellationToken cancellationToken = default)
    {
        await this.apiClient.PostAsync<Event>("api/events/update", eventDetails, cancellationToken);
        return true;
    }

    public async Task<bool> UpdateEnrollmentAsync(int eventId, int newEnrollmentCount, CancellationToken cancellationToken = default)
    {
        await this.apiClient.PostAsync<object>($"api/events/{eventId}/enrollment?count={newEnrollmentCount}", new { }, cancellationToken);
        return true;
    }

    public Task UpdateEventAsync(Event updatedEvent, CancellationToken cancellationToken = default) =>
        this.apiClient.PostAsync<Event>("api/events/update", updatedEvent, cancellationToken);

    public async Task<bool> DeleteAsync(int eventId, CancellationToken cancellationToken = default)
    {
        await this.apiClient.DeleteAsync($"api/events/{eventId}", cancellationToken);
        return true;
    }
}
