using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure.Repositories;

/// <summary>
/// Provides EF Core implementation for ambassador and referral operations.
/// </summary>
public sealed class AmbassadorRepository : IAmbassadorRepository
{
    private readonly MovieAppDbContext context;

    public AmbassadorRepository(MovieAppDbContext context)
    {
        this.context = context;
    }

    /// <inheritdoc/>
    public async Task<bool> IsReferralCodeValidAsync(string referralCode, CancellationToken ct = default)
    {
        return await this.context.AmbassadorProfiles
            .AnyAsync(ap => ap.PermanentCode == referralCode, ct);
    }

    /// <inheritdoc/>
    public async Task<string?> GetReferralCodeAsync(int userId, CancellationToken ct = default)
    {
        var profile = await this.context.AmbassadorProfiles
            .FirstOrDefaultAsync(ap => ap.UserId == userId, ct);
        return profile?.PermanentCode;
    }

    /// <inheritdoc/>
    public async Task CreateAmbassadorProfileAsync(int userId, string referralCode, CancellationToken ct = default)
    {
        var profile = new AmbassadorProfile
        {
            UserId = userId,
            PermanentCode = referralCode,
            RewardBalance = 0
        };
        this.context.AmbassadorProfiles.Add(profile);
        await this.context.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<int?> GetUserIdByReferralCodeAsync(string referralCode, CancellationToken ct = default)
    {
        var profile = await this.context.AmbassadorProfiles
            .FirstOrDefaultAsync(ap => ap.PermanentCode == referralCode, ct);
        return profile?.UserId;
    }

    /// <inheritdoc/>
    public async Task AddReferralLogAsync(int ambassadorId, int friendId, int eventId, CancellationToken ct = default)
    {
        var log = new ReferralLog
        {
            AmbassadorId = ambassadorId,
            ReferredUserId = friendId,
            EventId = eventId,
            CreatedAt = DateTime.UtcNow
        };
        this.context.ReferralLogs.Add(log);
        await this.context.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<bool> TryApplyRewardAsync(int ambassadorId, CancellationToken ct = default)
    {
        var profile = await this.context.AmbassadorProfiles
            .FirstOrDefaultAsync(ap => ap.UserId == ambassadorId, ct);

        if (profile == null) return false;

        profile.RewardBalance++;
        return await this.context.SaveChangesAsync(ct) > 0;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ReferralHistoryItem>> GetReferralHistoryAsync(int ambassadorId, CancellationToken ct = default)
    {
        return await this.context.ReferralLogs
            .Where(rl => rl.AmbassadorId == ambassadorId)
            .Include(rl => rl.ReferredUser)
            .Include(rl => rl.Event)
            .Select(rl => new ReferralHistoryItem
            {
                FriendName = rl.ReferredUser != null ? rl.ReferredUser.Username : "Unknown",
                EventTitle = rl.Event != null ? rl.Event.Title : "Deleted Event",
                UsedAt = rl.CreatedAt
            })
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<int> GetRewardBalanceAsync(int userId, CancellationToken ct = default)
    {
        var profile = await this.context.AmbassadorProfiles
            .FirstOrDefaultAsync(ap => ap.UserId == userId, ct);
        return profile?.RewardBalance ?? 0;
    }

    /// <inheritdoc/>
    public async Task DecrementRewardBalanceAsync(int userId, CancellationToken ct = default)
    {
        var profile = await this.context.AmbassadorProfiles
            .FirstOrDefaultAsync(ap => ap.UserId == userId, ct);

        if (profile != null && profile.RewardBalance > 0)
        {
            profile.RewardBalance--;
            await this.context.SaveChangesAsync(ct);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> HasReferralLogAsync(int ambassadorId, int friendId, int eventId, CancellationToken ct = default)
    {
        return await this.context.ReferralLogs
            .AnyAsync(rl => rl.AmbassadorId == ambassadorId &&
                            rl.ReferredUserId == friendId &&
                            rl.EventId == eventId, ct);
    }
}

