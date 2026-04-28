using System.Threading;
using System.Threading.Tasks;
#nullable enable

using MovieApp.Core.Models;

namespace MovieApp.Core.Interfaces.Service;

/// <summary>
/// Defines business logic for movie battle management and wagering operations.
/// </summary>
public interface IBattleService
{
    /// <summary>Gets the currently active battle.</summary>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    Task<Battle?> GetActiveBattleAsync(CancellationToken ct = default);

    /// <summary>Creates a new battle between two movies.</summary>
    /// <param name="firstMovieId">The identifier of the first movie.</param>
    /// <param name="secondMovieId">The identifier of the second movie.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    Task<Battle> CreateBattleAsync(int firstMovieId, int secondMovieId, CancellationToken ct = default);

    /// <summary>Places a bet on a battle outcome.</summary>
    /// <param name="userId">The identifier of the user placing the bet.</param>
    /// <param name="battleId">The identifier of the battle.</param>
    /// <param name="movieId">The identifier of the movie the user is betting on.</param>
    /// <param name="amount">The quantity of points to wager.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    Task<Bet> PlaceBetAsync(int userId, int battleId, int movieId, int amount, CancellationToken ct = default);

    /// <summary>Gets a user's specific bet for a given battle.</summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="battleId">The identifier of the battle.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    Task<Bet?> GetBetAsync(int userId, int battleId, CancellationToken ct = default);

    /// <summary>Determines the winning movie based on rating improvement.</summary>
    /// <param name="battleId">The identifier of the battle to evaluate.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    Task<int> DetermineWinnerAsync(int battleId, CancellationToken ct = default);

    /// <summary>Distributes point payouts to users who bet on the winning movie.</summary>
    /// <param name="battleId">The identifier of the battle to settle.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    Task DistributePayoutsAsync(int battleId, CancellationToken ct = default);

    /// <summary>Settles any active battles whose scheduled end date has passed.</summary>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    Task SettleExpiredBattlesAsync(CancellationToken ct = default);

    /// <summary>Gets the current battle, or the most recent battle the user engaged with.</summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    Task<Battle?> GetCurrentBattleForUserAsync(int userId, CancellationToken ct = default);

    /// <summary>Immediately settles a battle regardless of date for administrative purposes.</summary>
    Task ForceSettleBattleAsync(int battleId, CancellationToken ct = default);

    /// <summary>Resets all battle and betting data to a clean state.</summary>
    Task ResetAllBattlesForDemoAsync(CancellationToken ct = default);

    /// <summary>Generates a new battle using automated movie selection logic.</summary>
    Task<Battle> CreateDemoBattleAsync(CancellationToken ct = default);
}

