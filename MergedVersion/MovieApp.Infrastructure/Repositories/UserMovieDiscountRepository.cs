using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure.Repositories;

/// <summary>
/// Provides EF Core implementation for managing movie discount rewards.
/// </summary>
public sealed class UserMovieDiscountRepository : IUserMovieDiscountRepository
{
    private readonly MovieAppDbContext context;

    public UserMovieDiscountRepository(MovieAppDbContext context)
    {
        this.context = context;
    }

    /// <inheritdoc/>
    public async Task AddAsync(Reward reward, CancellationToken ct = default)
    {
        this.context.Rewards.Add(reward);
        await this.context.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<List<Reward>> GetDiscountsForUserAsync(int userIdentifier, CancellationToken ct = default)
    {
        return await this.context.Rewards
            .Where(r => r.OwnerUserId == userIdentifier)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task MarkRedeemedAsync(int rewardIdentifier, CancellationToken ct = default)
    {
        var reward = await this.context.Rewards.FindAsync(new object[] { rewardIdentifier }, ct);
        if (reward != null)
        {
            reward.Redeem();
            await this.context.SaveChangesAsync(ct);
        }
    }
}

