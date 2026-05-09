using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.Proxy;

public class RemoteCurrentUserService : ICurrentUserService
{
    private readonly ApiClient apiClient;
    private User? currentUser;

    public RemoteCurrentUserService(ApiClient apiClient) => this.apiClient = apiClient;

    public User CurrentUser => this.currentUser
        ?? throw new InvalidOperationException("Current user has not been initialized. Call InitializeAsync first.");

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (this.currentUser is not null) return;

        this.currentUser = await this.apiClient.GetAsync<User>("api/users/current", cancellationToken)
            ?? throw new InvalidOperationException("Could not retrieve the current user from the API.");
    }
}
