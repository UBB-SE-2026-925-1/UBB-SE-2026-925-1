using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.Proxy;

public class RemoteNotificationService : INotificationService
{
    private readonly ApiClient apiClient;

    public RemoteNotificationService(ApiClient apiClient)
    {
        this.apiClient = apiClient;
    }

    public async Task<IReadOnlyList<Notification>> GetNotificationsByUserIdAsync(
        int userIdentifier,
        CancellationToken cancellationToken = default)
    {
        return (await this.apiClient.GetAsync<IEnumerable<Notification>>(
                    $"api/notifications/user/{userIdentifier}",
                    cancellationToken))
               ?.ToList().AsReadOnly()
               ?? new List<Notification>().AsReadOnly();
    }

    public async Task RemoveNotificationAsync(
        int notificationIdentifier,
        CancellationToken cancellationToken = default)
    {
        await this.apiClient.DeleteAsync(
            $"api/notifications/{notificationIdentifier}",
            cancellationToken);
    }

    public async Task MarkAsReadOrRemoveAsync(
        int notificationIdentifier,
        CancellationToken cancellationToken = default)
    {
        await this.apiClient.PostAsync<object>(
            $"api/notifications/{notificationIdentifier}/read",
            new { },
            cancellationToken);
    }

    public Task GeneratePriceDropNotificationAsync(int eventIdentifier, string eventTitle, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task GenerateSeatsAvailableNotificationAsync(int eventIdentifier, string eventTitle, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<IReadOnlyList<Notification>> GetNotificationsByUserAsync(int userIdentifier, CancellationToken cancellationToken = default)
        => GetNotificationsByUserIdAsync(userIdentifier, cancellationToken);

    public Task NotifyPriceDropAsync(int eventIdentifier, decimal oldPrice, decimal newPrice, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task NotifySeatsAvailableAsync(int eventIdentifier, int newCapacity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}