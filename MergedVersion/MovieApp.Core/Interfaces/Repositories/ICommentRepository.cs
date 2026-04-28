using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;

namespace MovieApp.Core.Interfaces.Repository;

/// <summary>
/// Defines data access operations for user comments.
/// </summary>
public interface ICommentRepository
{
    /// <summary>Retrieves all comments.</summary>
    Task<List<Comment>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Retrieves a specific comment by its identifier.</summary>
    Task<Comment?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Posts a new comment and returns its identifier.</summary>
    Task<int> InsertAsync(Comment comment, CancellationToken ct = default);

    /// <summary>Updates the content of an existing comment.</summary>
    Task<bool> UpdateAsync(Comment comment, CancellationToken ct = default);

    /// <summary>Deletes a comment from the system.</summary>
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

