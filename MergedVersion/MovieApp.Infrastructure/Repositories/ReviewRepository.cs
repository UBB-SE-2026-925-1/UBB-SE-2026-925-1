using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Interfaces.Repository;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Repositories;

public sealed class ReviewRepository : IReviewRepository
{
    private readonly MovieAppDbContext context;

    public ReviewRepository(MovieAppDbContext context) => this.context = context;

    public async Task<List<Review>> GetAllAsync(CancellationToken ct = default)
        => await this.context.Reviews.Include(r => r.User).Include(r => r.Movie).ToListAsync(ct);

    public async Task<Review?> GetByIdAsync(int id, CancellationToken ct = default)
        => await this.context.Reviews
            .Include(r => r.User)
            .Include(r => r.Movie)
            .FirstOrDefaultAsync(r => r.ReviewId == id, ct);

    public async Task<int> InsertAsync(Review review, CancellationToken ct = default)
    {
        this.context.Reviews.Add(review);
        await this.context.SaveChangesAsync(ct);
        return review.ReviewId;
    }

    public async Task<bool> UpdateAsync(Review review, CancellationToken ct = default)
    {
        this.context.Reviews.Update(review);
        return await this.context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var review = await this.GetByIdAsync(id, ct);
        if (review == null) return false;
        this.context.Reviews.Remove(review);
        return await this.context.SaveChangesAsync(ct) > 0;
    }
}

