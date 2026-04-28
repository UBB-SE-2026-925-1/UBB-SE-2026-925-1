using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;

namespace MovieApp.Core.Interfaces.Repository;

/// <summary>
/// Defines data access operations for movie reviews.
/// </summary>
public interface IReviewRepository
{
    /// <summary>
    /// Retrieves all reviews stored in the system.
    /// </summary>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A list of all reviews.</returns>
    Task<List<Review>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves a specific review by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the review.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>The review entity if found; otherwise, null.</returns>
    Task<Review?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Persists a new review and returns its new unique identifier.
    /// </summary>
    /// <param name="review">The review entity to insert.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>The generated identifier for the new review.</returns>
    Task<int> InsertAsync(Review review, CancellationToken ct = default);

    /// <summary>
    /// Updates the details of an existing review.
    /// </summary>
    /// <param name="review">The review entity with updated values.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>True if the update was successful; otherwise, false.</returns>
    Task<bool> UpdateAsync(Review review, CancellationToken ct = default);

    /// <summary>
    /// Removes a review from the system.
    /// </summary>
    /// <param name="id">The identifier of the review to delete.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>True if the deletion was successful; otherwise, false.</returns>
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

