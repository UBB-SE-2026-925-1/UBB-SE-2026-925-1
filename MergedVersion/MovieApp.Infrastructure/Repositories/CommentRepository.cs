using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Interfaces.Repository;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Repositories;

public sealed class CommentRepository : ICommentRepository
{
    private readonly MovieAppDbContext context;

    public CommentRepository(MovieAppDbContext context) => this.context = context;

    public async Task<List<Comment>> GetAllAsync(CancellationToken ct = default)
        => await this.context.Comments
            .Include(c => c.Author)
            .ToListAsync(ct);

    public async Task<Comment?> GetByIdAsync(int id, CancellationToken ct = default)
        => await this.context.Comments
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.AuthorId == id, ct);

    public async Task<int> InsertAsync(Comment comment, CancellationToken ct = default)
    {
        this.context.Comments.Add(comment);
        await this.context.SaveChangesAsync(ct);
        return comment.AuthorId;
    }

    public async Task<bool> UpdateAsync(Comment comment, CancellationToken ct = default)
    {
        this.context.Comments.Update(comment);
        return await this.context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var comment = await this.context.Comments.FindAsync(new object[] { id }, ct);
        if (comment == null) return false;
        this.context.Comments.Remove(comment);
        return await this.context.SaveChangesAsync(ct) > 0;
    }
}

