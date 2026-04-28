// <copyright file="IUserRepository.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Repositories;

using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;

/// <summary>
/// Provides persistence operations for loading and finding application users.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Finds a user based on their external authentication provider and subject identity.
    /// </summary>
    /// <param name="authenticationProvider">The name of the external authentication provider.</param>
    /// <param name="authenticationSubject">The unique subject identifier from the provider.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The user entity if found; otherwise, null.</returns>
    Task<User?> FindByAuthIdentityAsync(string authenticationProvider, string authenticationSubject, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all users.</summary>
    Task<List<User>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Retrieves a user by their internal ID.</summary>
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Adds a new user and returns the new ID.</summary>
    Task<int> InsertAsync(User user, CancellationToken ct = default);

    /// <summary>Updates user details.</summary>
    Task<bool> UpdateAsync(User user, CancellationToken ct = default);

    /// <summary>Deletes a user.</summary>
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

}
