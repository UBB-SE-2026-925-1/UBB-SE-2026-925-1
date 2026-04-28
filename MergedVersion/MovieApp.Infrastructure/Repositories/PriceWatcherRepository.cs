using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure.Repositories;

public sealed class PriceWatcherRepository : IPriceWatcherRepository
{
    private readonly MovieAppDbContext context;

    public PriceWatcherRepository(MovieAppDbContext context) => this.context = context;

    public async Task<List<WatchedEvent>> GetAllWatchedEventsAsync()
        => await this.context.Set<WatchedEvent>().ToListAsync();

    public async Task<bool> AddWatchAsync(WatchedEvent watchedEvent)
    {
        this.context.Set<WatchedEvent>().Add(watchedEvent);
        return await this.context.SaveChangesAsync() > 0;
    }

    public async Task RemoveWatchAsync(int eventId)
    {
        var watch = await this.GetWatchAsync(eventId);
        if (watch != null)
        {
            this.context.Set<WatchedEvent>().Remove(watch);
            await this.context.SaveChangesAsync();
        }
    }

    public async Task<WatchedEvent?> GetWatchAsync(int eventId)
        => await this.context.Set<WatchedEvent>()
            .FirstOrDefaultAsync(w => w.EventId == eventId);

    public async Task<bool> IsWatchingAsync(int eventId)
        => await this.context.Set<WatchedEvent>()
            .AnyAsync(w => w.EventId == eventId);
}
