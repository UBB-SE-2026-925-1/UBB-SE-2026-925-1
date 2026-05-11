using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Repositories;

namespace MovieApp.Proxy;

public class RemoteUserEventAttendanceRepository : IUserEventAttendanceRepository
{
    private readonly ApiClient apiClient;

    public RemoteUserEventAttendanceRepository(ApiClient apiClient) => this.apiClient = apiClient;

    public async Task<IReadOnlyList<int>> GetJoinedEventIdsAsync(int userIdentifier, CancellationToken cancellationToken = default)
    {
        var result = await this.apiClient.GetAsync<IEnumerable<int>>($"api/events/user/{userIdentifier}/attendance", cancellationToken);
        return (result?.ToList() ?? new List<int>()).AsReadOnly();
    }

    public Task JoinAsync(int userIdentifier, int eventIdentifier, CancellationToken cancellationToken = default) =>
        this.apiClient.PostAsync<object>($"api/events/user/{userIdentifier}/attendance/join?eventId={eventIdentifier}", new { }, cancellationToken);

    public Task CancelAttendanceAsync(int userIdentifier, int eventIdentifier, CancellationToken cancellationToken = default) =>
        this.apiClient.DeleteAsync($"api/events/user/{userIdentifier}/attendance?eventId={eventIdentifier}", cancellationToken);
}
