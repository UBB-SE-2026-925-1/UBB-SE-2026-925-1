using System.Threading.Tasks;
#nullable enable

using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure.Repositories;

/// <summary>
/// Provides Entity Framework Core implementation for themed movie marathon tracking and leaderboards.
/// </summary>
public sealed class MarathonRepository : IMarathonRepository
{
    private readonly MovieAppDbContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarathonRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public MarathonRepository(MovieAppDbContext context)
    {
        this.context = context;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Marathon>> GetActiveMarathonsAsync()
        => await this.context.Marathons.Where(m => m.IsActive).ToListAsync();

    /// <inheritdoc/>
    public async Task<MarathonProgress?> GetUserProgressAsync(int userId, int marathonId)
        => await this.context.Set<MarathonProgress>()
            .FirstOrDefaultAsync(p => p.UserId == userId && p.MarathonId == marathonId);

    /// <inheritdoc/>
    public async Task<bool> JoinMarathonAsync(int userId, int marathonId)
    {
        var exists = await this.context.Set<MarathonProgress>()
            .AnyAsync(p => p.UserId == userId && p.MarathonId == marathonId);

        if (exists)
        {
            return false;
        }

        this.context.Set<MarathonProgress>().Add(new MarathonProgress
        {
            UserId = userId,
            MarathonId = marathonId,
            JoinedAt = DateTime.UtcNow,
            TriviaAccuracy = 0,
            CompletedMoviesCount = 0
        });

        return await this.context.SaveChangesAsync() > 0;
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateProgressAsync(MarathonProgress progress)
    {
        this.context.Set<MarathonProgress>().Update(progress);
        return await this.context.SaveChangesAsync() > 0;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MarathonProgress>> GetLeaderboardAsync(int marathonId)
        => await this.context.Set<MarathonProgress>()
            .Where(p => p.MarathonId == marathonId)
            .OrderByDescending(p => p.TriviaAccuracy)
            .ThenByDescending(p => p.CompletedMoviesCount)
            .ToListAsync();

    /// <inheritdoc/>
    public async Task<bool> IsPrerequisiteCompletedAsync(int userId, int prerequisiteId)
        => await this.context.Set<MarathonProgress>()
            .AnyAsync(p => p.UserId == userId && p.MarathonId == prerequisiteId && p.FinishedAt != null);

    /// <inheritdoc/>
    public async Task<int> GetMarathonMovieCountAsync(int marathonId)
    {
        var marathon = await this.context.Marathons
            .Include(m => m.Movies)
            .FirstOrDefaultAsync(m => m.Id == marathonId);

        return marathon?.Movies?.Count ?? 0;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Marathon>> GetWeeklyMarathonsForUserAsync(int userId, string weekString)
        => await this.context.Marathons
            .Where(m => m.WeekScoping == weekString)
            .ToListAsync();

    /// <inheritdoc/>
    public async Task AssignWeeklyMarathonsAsync(int userId, string weekString, int count = 10)
    {
        // This is typically a logic-heavy batch operation. 
        // Usually involves picking 'count' marathons for 'weekString' and creating Progress entries.
        var available = await this.context.Marathons
            .Where(m => m.WeekScoping == weekString && m.IsActive)
            .Take(count)
            .ToListAsync();

        foreach (var marathon in available)
        {
            await this.JoinMarathonAsync(userId, marathon.Id);
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MovieApp.Core.Models.Movie>> GetMoviesForMarathonAsync(int marathonId)
    {
        var marathon = await this.context.Marathons
            .Include(m => m.Movies)
            .FirstOrDefaultAsync(m => m.Id == marathonId);

        return marathon?.Movies ?? Enumerable.Empty<MovieApp.Core.Models.Movie>();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<LeaderboardEntry>> GetLeaderboardWithUsernamesAsync(int marathonId)
        => await this.context.Set<MarathonProgress>()
            .Where(p => p.MarathonId == marathonId)
            .Include(p => p.User)
            .OrderByDescending(p => p.CompletedMoviesCount)
            .ThenByDescending(p => p.TriviaAccuracy)
            .Select(p => new LeaderboardEntry
            {
                UserId = p.UserId,
                Username = p.User != null ? p.User.Username : "Unknown User",
                CompletedMoviesCount = p.CompletedMoviesCount,
                TriviaAccuracy = p.TriviaAccuracy,
                FinishedAt = p.FinishedAt
            })
            .ToListAsync();

    /// <inheritdoc/>
    public async Task<int> GetParticipantCountAsync(int marathonId)
        => await this.context.Set<MarathonProgress>()
            .CountAsync(p => p.MarathonId == marathonId);
}
