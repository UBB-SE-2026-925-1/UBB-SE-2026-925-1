using MovieApp.Core.Models;
using MovieApp.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.Proxy;
public class RemoteNotificationService : INotificationService
{
    private readonly ApiClient apiClient;
    public RemoteNotificationService(ApiClient apiClient) => this.apiClient = apiClient;
    public async Task<IReadOnlyList<Notification>> GetNotificationsByUserIdAsync(int userIdentifier, CancellationToken ct = default) =>
        (await this.apiClient.GetAsync<IEnumerable<Notification>>($"api/notifications/user/{userIdentifier}", ct))?.ToList().AsReadOnly() ?? new List<Notification>().AsReadOnly();
    public Task RemoveNotificationAsync(int notificationIdentifier, CancellationToken ct = default) => this.apiClient.DeleteAsync($"api/notifications/{notificationIdentifier}", ct);
    public Task MarkAsReadOrRemoveAsync(int notificationIdentifier, CancellationToken ct = default) => this.apiClient.PostAsync<object>($"api/notifications/{notificationIdentifier}/read", new { }, ct);

    public Task GeneratePriceDropNotificationAsync(int eventIdentifier, string eventTitle, CancellationToken ct = default) => throw new NotImplementedException();
    public Task GenerateSeatsAvailableNotificationAsync(int eventIdentifier, string eventTitle, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<IReadOnlyList<Notification>> GetNotificationsByUserAsync(int userIdentifier, CancellationToken ct = default) => GetNotificationsByUserIdAsync(userIdentifier, ct);
    public Task NotifyPriceDropAsync(int eventIdentifier, decimal oldPrice, decimal newPrice, CancellationToken ct = default) => throw new NotImplementedException();
    public Task NotifySeatsAvailableAsync(int eventIdentifier, int newCapacity, CancellationToken ct = default) => throw new NotImplementedException();
}