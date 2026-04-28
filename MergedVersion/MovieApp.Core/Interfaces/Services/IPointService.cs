using System.Threading;
using System.Threading.Tasks;
#nullable enable

using MovieApp.Core.Models;

namespace MovieApp.Core.Interfaces.Service;

/// <summary>
/// Defines business logic for user points, scoring, and betting balances.
/// </summary>
public interface IPointService
{
    /// <summary>
    /// Retrieves the statistics and point balance for a specific user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>The user's statistics entity.</returns>
    Task<UserStats> GetUserStatsAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Awards points to a user based on movie interactions.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="movieId">The identifier of the movie interacted with.</param>
    /// <param name="isBattleMovie">Indicates if the movie is part of an active battle.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddPointsAsync(int userId, int movieId, bool isBattleMovie, CancellationToken ct = default);

    /// <summary>
    /// Deducts a specified amount of points from a user's balance.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="points">The number of points to deduct.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeductPointsAsync(int userId, int points, CancellationToken ct = default);

    /// <summary>
    /// Freezes points to cover a pending bet.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="amount">The amount to freeze.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task FreezePointsAsync(int userId, int amount, CancellationToken ct = default);

    /// <summary>
    /// Refunds frozen points back to the user's active balance.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="amount">The amount to refund.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RefundPointsAsync(int userId, int amount, CancellationToken ct = default);

    /// <summary>
    /// Re-evaluates and updates the user's ranking score for the current week.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateWeeklyScoreAsync(int userId, CancellationToken ct = default);
}

