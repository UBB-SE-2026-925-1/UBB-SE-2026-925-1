using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;

namespace MovieApp.Proxy;

public class RemoteCommentService : ICommentService
{
    private readonly ApiClient apiClient;

    public RemoteCommentService(ApiClient apiClient) => this.apiClient = apiClient;

    public async Task<List<Comment>> GetCommentsForMovieAsync(int movieId, CancellationToken ct = default)
    {
        var result = await this.apiClient.GetAsync<IEnumerable<Comment>>($"api/comments/movie/{movieId}", ct);
        return result?.ToList() ?? new List<Comment>();
    }

    public async Task<Comment> AddCommentAsync(int userId, int movieId, string content, CancellationToken ct = default)
    {
        var result = await this.apiClient.PostAsync<object, Comment>(
            "api/comments",
            new { UserId = userId, MovieId = movieId, Content = content },
            ct);
        return result ?? throw new Exception("Failed to add comment.");
    }

    public async Task<Comment> AddReplyAsync(int userId, int parentCommentId, string content, CancellationToken ct = default)
    {
        var result = await this.apiClient.PostAsync<object, Comment>(
            "api/comments/reply",
            new { UserId = userId, ParentCommentId = parentCommentId, Content = content },
            ct);
        return result ?? throw new Exception("Failed to add reply.");
    }

    public Task DeleteCommentAsync(int commentId, CancellationToken ct = default)
        => this.apiClient.DeleteAsync($"api/comments/{commentId}", ct);
}