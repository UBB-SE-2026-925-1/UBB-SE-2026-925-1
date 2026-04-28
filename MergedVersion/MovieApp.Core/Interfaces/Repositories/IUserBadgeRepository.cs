using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;

namespace MovieApp.Core.Interfaces.Repository;

/// <summary>
/// Defines data access operations for the association between users and badges.
/// </summary>
public interface IUserBadgeRepository
{
    /// <summary>Retrieves all user-badge associations.</summary>
    Task<List<UserBadge>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Retrieves a specific badge assignment for a user.</summary>
    Task<UserBadge?> GetByIdAsync(int userId, int badgeId, CancellationToken ct = default);

    /// <summary>Assigns a badge to a user.</summary>
    Task<bool> InsertAsync(UserBadge userBadge, CancellationToken ct = default);

    /// <summary>Updates a user-badge association record.</summary>
    Task<bool> UpdateAsync(UserBadge userBadge, CancellationToken ct = default);

    /// <summary>Removes a badge assignment from a user.</summary>
    Task<bool> DeleteAsync(int userId, int badgeId, CancellationToken ct = default);
}

