using System;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.UI.Services.Api;

public class RemoteCurrentUserService : ICurrentUserService
{
    private readonly ApiClient apiClient;
    private User? currentUser;

    public RemoteCurrentUserService(ApiClient apiClient)
    {
        this.apiClient = apiClient;
    }

    public User CurrentUser => this.currentUser ?? throw new InvalidOperationException("User not initialized");

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (this.currentUser != null) return;
        
        this.currentUser = await this.apiClient.GetAsync<User>("api/users/current", cancellationToken);
        
        if (this.currentUser == null)
        {
            throw new Exception("Could not retrieve current user from API");
        }
    }
}
