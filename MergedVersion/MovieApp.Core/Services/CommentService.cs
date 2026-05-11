using System.Threading;
using System.Threading.Tasks;
#nullable enable

using MovieApp.Core.Interfaces.Repository;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Core.Services;

/// <summary>
/// Provides logic for threaded comments and forum interactions.
/// </summary>
public sealed class CommentService : ICommentService
{
    /// <summary>Maximum number of characters allowed in a single comment or reply.
    /// Mirrors the database column length defined in CommentConfiguration.</summary>
    private const int MaxCommentContentLength = 10000;

    private readonly ICommentRepository commentRepository;
    private readonly IUserRepository userRepository;
    private readonly IMovieRepository movieRepository;

    public CommentService(
        ICommentRepository commentRepository,
        IUserRepository userRepository,
        IMovieRepository movieRepository)
    {
        this.commentRepository = commentRepository;
        this.userRepository = userRepository;
        this.movieRepository = movieRepository;
    }

    /// <inheritdoc/>
    public async Task<List<Comment>> GetCommentsForMovieAsync(int movieId, CancellationToken ct = default)
    {
        var allComments = await this.commentRepository.GetAllAsync(ct);
        return allComments
            .Where(c => c.MovieId == movieId)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<Comment> AddCommentAsync(int userId, int movieId, string content, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Comment content cannot be empty.");
        }

        if (content.Length > MaxCommentContentLength)
        {
            throw new InvalidOperationException("Comment content exceeds maximum length.");
        }

        // Validate the user and movie exist before inserting
        var existingAuthor = await this.userRepository.GetByIdAsync(userId, ct)
            ?? throw new InvalidOperationException("User not found.");
        var existingMovie = await this.movieRepository.GetByIdAsync(movieId, ct)
            ?? throw new InvalidOperationException("Movie not found.");

        var comment = new Comment
        {
            AuthorId = existingAuthor.Id,
            MovieId = existingMovie.Id,
            Content = content,
            CreatedAt = DateTime.UtcNow,
        };

        await this.commentRepository.InsertAsync(comment, ct);
        return comment;
    }

    /// <inheritdoc/>
    public async Task<Comment> AddReplyAsync(int userId, int parentCommentId, string content, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Reply content cannot be empty.");
        }

        if (content.Length > MaxCommentContentLength)
        {
            throw new InvalidOperationException("Reply content exceeds maximum length.");
        }

        var parentComment = await this.commentRepository.GetByIdAsync(parentCommentId, ct)
            ?? throw new InvalidOperationException("Parent comment not found.");

        var existingAuthor = await this.userRepository.GetByIdAsync(userId, ct)
            ?? throw new InvalidOperationException("User not found.");

        // Inherit the parent's MovieId so replies always live under the same movie thread,
        // even when navigation properties weren't loaded.
        var reply = new Comment
        {
            AuthorId = existingAuthor.Id,
            MovieId = parentComment.MovieId,
            ParentCommentId = parentComment.MessageId,
            Content = content,
            CreatedAt = DateTime.UtcNow,
        };

        await this.commentRepository.InsertAsync(reply, ct);
        return reply;
    }

    /// <inheritdoc/>
    public async Task DeleteCommentAsync(int commentId, CancellationToken ct = default)
    {
        await this.commentRepository.DeleteAsync(commentId, ct);
    }
}

