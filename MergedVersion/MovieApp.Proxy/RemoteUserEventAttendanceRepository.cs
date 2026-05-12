using MovieApp.Core.Repositories;

namespace MovieApp.Proxy;
public class RemoteUserEventAttendanceRepository : IUserEventAttendanceRepository
{
    private readonly ApiClient apiClient;
    public RemoteUserEventAttendanceRepository(ApiClient apiClient) => this.apiClient = apiClient;

    public async Task<IReadOnlyList<int>> GetJoinedEventIdsAsync(int userIdentifier, CancellationToken ct = default) =>
        (await this.apiClient.GetAsync<List<int>>($"api/events/user/{userIdentifier}/attendance", ct))?.AsReadOnly() ?? new List<int>().AsReadOnly();

    public Task JoinAsync(int userIdentifier, int eventIdentifier, CancellationToken ct = default) =>
        this.apiClient.PostAsync<object>($"api/events/user/{userIdentifier}/attendance/join?eventId={eventIdentifier}", new { }, ct);

    public Task CancelAttendanceAsync(int userIdentifier, int eventIdentifier, CancellationToken ct = default) =>
        this.apiClient.DeleteAsync($"api/events/user/{userIdentifier}/attendance?eventId={eventIdentifier}", ct);
}

