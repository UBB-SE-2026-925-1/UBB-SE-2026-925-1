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
            .Where(c => c.Movie?.Id == movieId)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<Comment> AddCommentAsync(int userId, int movieId, string content, CancellationToken ct = default)
    {
        if (content.Length > 10000)
        {
            throw new InvalidOperationException("Comment content exceeds maximum length.");
        }

        var author = await this.userRepository.GetByIdAsync(userId, ct)
            ?? throw new InvalidOperationException("User not found.");
        var movie = await this.movieRepository.GetByIdAsync(movieId, ct)
            ?? throw new InvalidOperationException("Movie not found.");

        var comment = new Comment
        {
            Author = author,
            Movie = movie,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        await this.commentRepository.InsertAsync(comment, ct);
        return comment;
    }

    /// <inheritdoc/>
    public async Task<Comment> AddReplyAsync(int userId, int parentCommentId, string content, CancellationToken ct = default)
    {
        var parent = await this.commentRepository.GetByIdAsync(parentCommentId, ct)
            ?? throw new InvalidOperationException("Parent comment not found.");

        var author = await this.userRepository.GetByIdAsync(userId, ct)
            ?? throw new InvalidOperationException("User not found.");

        var reply = new Comment
        {
            Author = author,
            Movie = parent.Movie,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            ParentComment = parent
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

