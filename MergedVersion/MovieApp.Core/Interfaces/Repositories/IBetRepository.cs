using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;

namespace MovieApp.Core.Interfaces.Repository;

/// <summary>
/// Defines data access operations for user bets on battles.
/// </summary>
public interface IBetRepository
{
    /// <summary>Retrieves all placed bets.</summary>
    Task<List<Bet>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Retrieves a specific bet by user and battle identifiers.</summary>
    Task<Bet?> GetByIdAsync(int userId, int battleId, CancellationToken ct = default);

    /// <summary>Places a new bet.</summary>
    Task<bool> InsertAsync(Bet bet, CancellationToken ct = default);

    /// <summary>Updates an existing bet.</summary>
    Task<bool> UpdateAsync(Bet bet, CancellationToken ct = default);

    /// <summary>Removes a specific bet.</summary>
    Task<bool> DeleteAsync(int userId, int battleId, CancellationToken ct = default);

    /// <summary>Removes all bets associated with a specific battle.</summary>
    Task<bool> DeleteByBattleIdAsync(int battleId, CancellationToken ct = default);
}

