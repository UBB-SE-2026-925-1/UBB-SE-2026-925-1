using System.Threading;
using System.Threading.Tasks;
#nullable enable

using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure.Repositories;

/// <summary>
/// Provides Entity Framework Core implementation for movie events and screening metadata.
/// </summary>
public sealed class EventRepository : IEventRepository
{
    private readonly MovieAppDbContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public EventRepository(MovieAppDbContext context)
    {
        this.context = context;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Event>> GetAllAsync(CancellationToken ct = default)
    {
        return await this.context.Events.AsNoTracking().ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Event>> GetAllByTypeAsync(string eventType, CancellationToken ct = default)
    {
        return await this.context.Events
            .Where(e => e.EventType == eventType)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<Event?> FindByIdAsync(int eventId, CancellationToken ct = default)
    {
        return await this.context.Events.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == eventId, ct);
    }

    /// <inheritdoc/>
    public async Task<int> AddAsync(Event eventDetails, CancellationToken ct = default)
    {
        this.context.Events.Add(eventDetails);
        await this.context.SaveChangesAsync(ct);
        return eventDetails.Id;
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(Event eventDetails, CancellationToken ct = default)
    {
        this.context.Events.Update(eventDetails);
        return await this.context.SaveChangesAsync(ct) > 0;
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateEnrollmentAsync(int eventId, int newEnrollmentCount, CancellationToken ct = default)
    {
        var eventEntity = await this.FindByIdAsync(eventId, ct);
        if (eventEntity == null)
        {
            return false;
        }

        eventEntity.CurrentEnrollment = newEnrollmentCount;
        return await this.context.SaveChangesAsync(ct) > 0;
    }

    /// <inheritdoc/>
    public async Task UpdateEventAsync(Event updatedEvent, CancellationToken ct = default)
    {
        this.context.Events.Update(updatedEvent);
        await this.context.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int eventId, CancellationToken ct = default)
    {
        var eventEntity = await this.FindByIdAsync(eventId, ct);
        if (eventEntity == null)
        {
            return false;
        }

        this.context.Events.Remove(eventEntity);
        return await this.context.SaveChangesAsync(ct) > 0;
    }
}

