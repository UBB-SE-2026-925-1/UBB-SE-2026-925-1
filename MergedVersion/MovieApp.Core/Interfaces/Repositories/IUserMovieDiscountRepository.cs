// <copyright file="IUserMovieDiscountRepository.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Repositories;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;

/// <summary>
/// Provides persistence operations for movie discount rewards.
/// </summary>
public interface IUserMovieDiscountRepository
{
    /// <summary>
    /// Adds a new discount reward to the user's collection.
    /// </summary>
    /// <param name="reward">The reward entity to save.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(Reward reward, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all discount rewards associated with a specific user.
    /// </summary>
    /// <param name="userIdentifier">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A list of rewards belonging to the user.</returns>
    Task<List<Reward>> GetDiscountsForUserAsync(int userIdentifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists the redeemed state for a reward so it cannot be used again.
    /// </summary>
    /// <param name="rewardIdentifier">The unique identifier of the reward.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MarkRedeemedAsync(int rewardIdentifier, CancellationToken cancellationToken = default);
}
