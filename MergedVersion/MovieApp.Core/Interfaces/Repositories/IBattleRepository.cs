using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;

namespace MovieApp.Core.Interfaces.Repository;

/// <summary>
/// Defines data access operations for movie battles.
/// </summary>
public interface IBattleRepository
{
    /// <summary>Retrieves all movie battles.</summary>
    Task<List<Battle>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Retrieves a specific battle by its identifier.</summary>
    Task<Battle?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Creates a new battle and returns its identifier.</summary>
    Task<int> InsertAsync(Battle battle, CancellationToken ct = default);

    /// <summary>Updates battle details or status.</summary>
    Task<bool> UpdateAsync(Battle battle, CancellationToken ct = default);

    /// <summary>Removes a battle record.</summary>
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

