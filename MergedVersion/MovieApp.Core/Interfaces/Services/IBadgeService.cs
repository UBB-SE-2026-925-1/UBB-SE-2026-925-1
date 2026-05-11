using System.Threading;
using System.Threading.Tasks;
#nullable enable

using MovieApp.Core.Models;
using MovieApp.WebAPI.Controllers.DTOs;


namespace MovieApp.Core.Interfaces.Service;

/// <summary>
/// Defines business logic operations for badge/achievement management and awarding.
/// </summary>
public interface IBadgeService
{
    /// <summary>
    /// Retrieves all badges earned by a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A list of badges the user has earned.</returns>
    Task<UserBadgesDTO> GetUserBadgesAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all available badges defined in the system.
    /// </summary>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A list of all badges.</returns>
    Task<List<Badge>> GetAllBadgesAsync(CancellationToken ct = default);

    /// <summary>
    /// Evaluates badge criteria for a user and persists any newly earned achievements.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to evaluate.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CheckAndAwardBadgesAsync(int userId, CancellationToken ct = default);
}

