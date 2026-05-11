using MovieApp.Core.DTOs;
using MovieApp.Core.Services;

namespace MovieApp.Proxy;

public class RemoteCurrentUserService : ICurrentUserService
{
    private readonly ApiClient apiClient;
    private CurrentUserDTO? currentUser;

    public RemoteCurrentUserService(ApiClient apiClient)
    {
        this.apiClient = apiClient;
    }

    public CurrentUserDTO CurrentUser => this.currentUser
        ?? throw new InvalidOperationException(
            "Current user has not been initialized. Call InitializeAsync first.");

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        if (this.currentUser is not null)
        {
            return;
        }

        this.currentUser = await this.apiClient.GetAsync<CurrentUserDTO>(
            "api/users/current",
            cancellationToken)
            ?? throw new InvalidOperationException(
                "Could not retrieve the current user from the API.");
    }
}