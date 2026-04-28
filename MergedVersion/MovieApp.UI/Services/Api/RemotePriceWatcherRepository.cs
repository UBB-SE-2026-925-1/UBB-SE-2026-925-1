using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.UI.Services.Api;

public class RemotePriceWatcherRepository : IPriceWatcherRepository
{
    private readonly ApiClient apiClient;

    public RemotePriceWatcherRepository(ApiClient apiClient)
    {
        this.apiClient = apiClient;
    }

    public async Task<List<WatchedEvent>> GetAllWatchedEventsAsync()
    {
        var result = await this.apiClient.GetAsync<IEnumerable<WatchedEvent>>("api/pricewatcher");
        return result?.ToList() ?? new List<WatchedEvent>();
    }

    public async Task<bool> AddWatchAsync(WatchedEvent watchedEvent)
    {
        return await this.apiClient.PostAsync<WatchedEvent, bool>("api/pricewatcher", watchedEvent);
    }

    public async Task RemoveWatchAsync(int eventIdentifier)
    {
        await this.apiClient.DeleteAsync($"api/pricewatcher/{eventIdentifier}");
    }

    public async Task<WatchedEvent?> GetWatchAsync(int eventIdentifier)
    {
        return await this.apiClient.GetAsync<WatchedEvent>($"api/pricewatcher/{eventIdentifier}");
    }

    public async Task<bool> IsWatchingAsync(int eventIdentifier)
    {
        return await this.apiClient.GetAsync<bool>($"api/pricewatcher/check/{eventIdentifier}");
    }
}
