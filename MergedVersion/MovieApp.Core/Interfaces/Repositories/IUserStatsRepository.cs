using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;

namespace MovieApp.Core.Interfaces.Repository;

/// <summary>
/// Defines data access operations for user statistics.
/// </summary>
public interface IUserStatsRepository
{
    /// <summary>Retrieves all user statistics records.</summary>
    Task<List<UserStats>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Retrieves a specific user stats record by its primary identifier.</summary>
    Task<UserStats?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Retrieves statistics associated with a specific user identifier.</summary>
    Task<UserStats?> GetByUserIdAsync(int userId, CancellationToken ct = default);

    /// <summary>Persists a new user stats record and returns the new identifier.</summary>
    Task<int> InsertAsync(UserStats userStats, CancellationToken ct = default);

    /// <summary>Updates an existing user stats record.</summary>
    Task<bool> UpdateAsync(UserStats userStats, CancellationToken ct = default);

    /// <summary>Removes a user stats record from the system.</summary>
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

