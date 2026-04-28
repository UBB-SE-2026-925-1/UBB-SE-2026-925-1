using System.Threading;
using System.Threading.Tasks;
#nullable enable

using MovieApp.Core.Models;

namespace MovieApp.Core.Interfaces.Service;

/// <summary>
/// Defines business logic for movie reviews and rating calculations.
/// </summary>
public interface IReviewService
{
    /// <summary>
    /// Retrieves all reviews for a specific movie.
    /// </summary>
    /// <param name="movieId">The identifier of the movie.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A list of reviews.</returns>
    Task<List<Review>> GetReviewsForMovieAsync(int movieId, CancellationToken ct = default);

    /// <summary>
    /// Submits a new review for a movie.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="movieId">The identifier of the movie.</param>
    /// <param name="rating">The numeric rating given by the user.</param>
    /// <param name="content">The text content of the review.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>The created review entity.</returns>
    Task<Review> AddReviewAsync(int userId, int movieId, float rating, string content, CancellationToken ct = default);

    /// <summary>
    /// Updates the content or rating of an existing review.
    /// </summary>
    /// <param name="reviewId">The identifier of the review.</param>
    /// <param name="rating">The updated rating.</param>
    /// <param name="content">The updated text content.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateReviewAsync(int reviewId, float rating, string content, CancellationToken ct = default);

    /// <summary>
    /// Removes a review from the system.
    /// </summary>
    /// <param name="reviewId">The identifier of the review.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteReviewAsync(int reviewId, CancellationToken ct = default);

    /// <summary>
    /// Submits an extended review with detailed category scores.
    /// </summary>
    /// <param name="reviewId">The identifier of the existing review to extend.</param>
    /// <param name="cgRating">Rating for CGI/Visuals.</param>
    /// <param name="cgText">Comments on CGI/Visuals.</param>
    /// <param name="actingRating">Rating for Acting.</param>
    /// <param name="actingText">Comments on Acting.</param>
    /// <param name="plotRating">Rating for Plot.</param>
    /// <param name="plotText">Comments on Plot.</param>
    /// <param name="soundRating">Rating for Sound/Music.</param>
    /// <param name="soundText">Comments on Sound/Music.</param>
    /// <param name="cinRating">Rating for Cinematography.</param>
    /// <param name="cinText">Comments on Cinematography.</param>
    /// <param name="mainExtraText">General extended commentary.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SubmitExtraReviewAsync(
        int reviewId,
        int cgRating, string cgText,
        int actingRating, string actingText,
        int plotRating, string plotText,
        int soundRating, string soundText,
        int cinRating, string cinText,
        string mainExtraText,
        CancellationToken ct = default);

    /// <summary>
    /// Calculates the current average rating for a movie based on all reviews.
    /// </summary>
    /// <param name="movieId">The identifier of the movie.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>The calculated average rating.</returns>
    Task<double> GetAverageRatingAsync(int movieId, CancellationToken ct = default);
}

