using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure.Repositories;

public sealed class FavoriteEventRepository : IFavoriteEventRepository
{
    private readonly MovieAppDbContext context;

    public FavoriteEventRepository(MovieAppDbContext context) => this.context = context;

    public async Task AddAsync(int userId, int eventId, CancellationToken ct = default)
    {
        var favorite = new FavoriteEvent { UserId = userId, EventId = eventId };
        this.context.Set<FavoriteEvent>().Add(favorite);
        await this.context.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(int userId, int eventId, CancellationToken ct = default)
    {
        var favorite = await this.context.Set<FavoriteEvent>()
            .FirstOrDefaultAsync(f => f.UserId == userId && f.EventId == eventId, ct);
        if (favorite != null)
        {
            this.context.Set<FavoriteEvent>().Remove(favorite);
            await this.context.SaveChangesAsync(ct);
        }
    }

    public async Task<IReadOnlyList<FavoriteEvent>> FindByUserAsync(int userId, CancellationToken ct = default)
        => await this.context.Set<FavoriteEvent>()
            .Where(f => f.UserId == userId)
            .ToListAsync(ct);

    public async Task<bool> ExistsAsync(int userId, int eventId, CancellationToken ct = default)
        => await this.context.Set<FavoriteEvent>()
            .AnyAsync(f => f.UserId == userId && f.EventId == eventId, ct);

    public async Task<IReadOnlyList<int>> GetUsersByFavoriteEventAsync(int eventId, CancellationToken ct = default)
        => await this.context.Set<FavoriteEvent>()
            .Where(f => f.EventId == eventId)
            .Select(f => f.UserId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<FavoriteEvent>> FindByEventAsync(int eventId, CancellationToken ct = default)
        => await this.context.Set<FavoriteEvent>()
            .Where(f => f.EventId == eventId)
            .ToListAsync(ct);
}


