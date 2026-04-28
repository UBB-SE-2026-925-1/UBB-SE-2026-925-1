using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Interfaces.Repository;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Repositories;

/// <summary>
/// Provides EF Core implementation for managing user statistics and point totals.
/// </summary>
public sealed class UserStatsRepository : IUserStatsRepository
{
    private readonly MovieAppDbContext context;

    public UserStatsRepository(MovieAppDbContext context)
    {
        this.context = context;
    }

    /// <inheritdoc/>
    public async Task<List<UserStats>> GetAllAsync(CancellationToken ct = default)
        => await this.context.UserStats.Include(s => s.User).ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<UserStats?> GetByIdAsync(int id, CancellationToken ct = default)
        => await this.context.UserStats
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StatsId == id, ct);

    /// <inheritdoc/>
    public async Task<UserStats?> GetByUserIdAsync(int userId, CancellationToken ct = default)
        => await this.context.UserStats
            .FirstOrDefaultAsync(s => s.User.Id == userId, ct);

    /// <inheritdoc/>
    public async Task<int> InsertAsync(UserStats userStats, CancellationToken ct = default)
    {
        this.context.UserStats.Add(userStats);
        await this.context.SaveChangesAsync(ct);
        return userStats.StatsId;
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(UserStats userStats, CancellationToken ct = default)
    {
        this.context.UserStats.Update(userStats);
        return await this.context.SaveChangesAsync(ct) > 0;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var stats = await this.context.UserStats.FindAsync(new object[] { id }, ct);
        if (stats == null) return false;

        this.context.UserStats.Remove(stats);
        return await this.context.SaveChangesAsync(ct) > 0;
    }
}

