using System.Threading;
using System.Threading.Tasks;
#nullable enable

using MovieApp.Core.Interfaces.Repository;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Core.Services;

/// <summary>
/// Manages user point balances, scoring logic, and betting transactions.
/// </summary>
public sealed class PointService : IPointService
{
    private readonly IUserStatsRepository userStatsRepository;
    private readonly IUserRepository userRepository;
    private readonly IMovieRepository movieRepository;
    private readonly IBadgeService badgeService;

    public PointService(
        IUserStatsRepository userStatsRepository,
        IUserRepository userRepository,
        IMovieRepository movieRepository,
        IBadgeService badgeService)
    {
        this.userStatsRepository = userStatsRepository;
        this.userRepository = userRepository;
        this.movieRepository = movieRepository;
        this.badgeService = badgeService;
    }

    /// <inheritdoc/>
    public async Task<UserStats> GetUserStatsAsync(int userId, CancellationToken ct = default)
    {
        var stats = await this.userStatsRepository.GetByUserIdAsync(userId, ct);

        if (stats == null)
        {
            var user = await this.userRepository.GetByIdAsync(userId, ct)
                ?? throw new InvalidOperationException("User not found.");

            stats = new UserStats { User = user, TotalPoints = 0, WeeklyScore = 0 };
            await this.userStatsRepository.InsertAsync(stats, ct);
        }

        return stats;
    }

    /// <inheritdoc/>
    public async Task AddPointsAsync(int userId, int movieId, bool isBattleMovie, CancellationToken ct = default)
    {
        var stats = await this.GetUserStatsAsync(userId, ct);
        var movie = await this.movieRepository.GetByIdAsync(movieId, ct);

        if (movie == null) return;

        int pointsToAdd = isBattleMovie ? 5 : 0;
        if (movie.AverageRating > 3.5) pointsToAdd += 2;
        else if (movie.AverageRating < 2.0) pointsToAdd += 1;

        stats.TotalPoints = Math.Max(0, stats.TotalPoints + pointsToAdd);
        await this.userStatsRepository.UpdateAsync(stats, ct);

        await this.badgeService.CheckAndAwardBadgesAsync(userId, ct);
    }

    /// <inheritdoc/>
    public async Task DeductPointsAsync(int userId, int points, CancellationToken ct = default)
    {
        var stats = await this.GetUserStatsAsync(userId, ct);
        stats.TotalPoints = Math.Max(0, stats.TotalPoints - points);
        await this.userStatsRepository.UpdateAsync(stats, ct);
    }

    /// <inheritdoc/>
    public async Task FreezePointsAsync(int userId, int amount, CancellationToken ct = default)
    {
        var stats = await this.GetUserStatsAsync(userId, ct);
        if (stats.TotalPoints < amount)
        {
            throw new InvalidOperationException($"Insufficient points. Required: {amount}, available: {stats.TotalPoints}.");
        }

        stats.TotalPoints -= amount;
        await this.userStatsRepository.UpdateAsync(stats, ct);
    }

    /// <inheritdoc/>
    public async Task RefundPointsAsync(int userId, int amount, CancellationToken ct = default)
    {
        var stats = await this.GetUserStatsAsync(userId, ct);
        stats.TotalPoints += amount;
        await this.userStatsRepository.UpdateAsync(stats, ct);
    }

    /// <inheritdoc/>
    public async Task UpdateWeeklyScoreAsync(int userId, CancellationToken ct = default)
    {
        var stats = await this.GetUserStatsAsync(userId, ct);
        stats.WeeklyScore = stats.TotalPoints;
        await this.userStatsRepository.UpdateAsync(stats, ct);
    }
}

