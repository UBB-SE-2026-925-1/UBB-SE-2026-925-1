using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure.Repositories;

public sealed class TriviaRewardRepository : ITriviaRewardRepository
{
    private readonly MovieAppDbContext context;

    public TriviaRewardRepository(MovieAppDbContext context) => this.context = context;

    /// <inheritdoc/>
    public async Task AddAsync(TriviaReward reward, CancellationToken ct = default)
    {
        this.context.Set<TriviaReward>().Add(reward);
        await this.context.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<TriviaReward?> GetUnredeemedByUserAsync(int userIdentifier, CancellationToken ct = default)
    {
        return await this.context.Set<TriviaReward>()
            .Where(r => r.UserId == userIdentifier && !r.IsRedeemed)
            .OrderBy(r => r.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task MarkAsRedeemedAsync(int rewardIdentifier, CancellationToken ct = default)
    {
        var reward = await this.context.Set<TriviaReward>().FindAsync(new object[] { rewardIdentifier }, ct);
        if (reward != null)
        {
            reward.IsRedeemed = true;
            await this.context.SaveChangesAsync(ct);
        }
    }
}

