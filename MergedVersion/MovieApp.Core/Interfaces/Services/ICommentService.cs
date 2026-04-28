using System.Threading;
using System.Threading.Tasks;
#nullable enable

using MovieApp.Core.Models;

namespace MovieApp.Core.Interfaces.Service;

/// <summary>
/// Defines business logic for movie forum and threaded comment operations.
/// </summary>
public interface ICommentService
{
    /// <summary>
    /// Retrieves all comments for a specific movie, ordered by creation date descending.
    /// </summary>
    /// <param name="movieId">The identifier of the movie.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A list of comments associated with the movie.</returns>
    Task<List<Comment>> GetCommentsForMovieAsync(int movieId, CancellationToken ct = default);

    /// <summary>
    /// Adds a new root-level comment to a movie.
    /// </summary>
    /// <param name="userId">The identifier of the user posting the comment.</param>
    /// <param name="movieId">The identifier of the movie.</param>
    /// <param name="content">The text content of the comment.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>The created comment entity.</returns>
    Task<Comment> AddCommentAsync(int userId, int movieId, string content, CancellationToken ct = default);

    /// <summary>
    /// Adds a reply to an existing comment.
    /// </summary>
    /// <param name="userId">The identifier of the user replying.</param>
    /// <param name="parentCommentId">The identifier of the comment being replied to.</param>
    /// <param name="content">The text content of the reply.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>The created reply comment entity.</returns>
    Task<Comment> AddReplyAsync(int userId, int parentCommentId, string content, CancellationToken ct = default);

    /// <summary>
    /// Removes a comment from the system.
    /// </summary>
    /// <param name="commentId">The identifier of the comment to delete.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteCommentAsync(int commentId, CancellationToken ct = default);
}

