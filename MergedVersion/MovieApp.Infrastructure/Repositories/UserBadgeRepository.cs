using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Interfaces.Repository;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Repositories;

public sealed class UserBadgeRepository : IUserBadgeRepository
{
    private readonly MovieAppDbContext context;

    public UserBadgeRepository(MovieAppDbContext context) => this.context = context;

    public async Task<List<UserBadge>> GetAllAsync(CancellationToken ct = default)
        => await this.context.UserBadges.Include(ub => ub.User).Include(ub => ub.Badge).ToListAsync(ct);

    public async Task<UserBadge?> GetByIdAsync(int userId, int badgeId, CancellationToken ct = default)
        => await this.context.UserBadges
            .FirstOrDefaultAsync(ub => ub.User.Id == userId && ub.Badge.BadgeId == badgeId, ct);

    public async Task<bool> InsertAsync(UserBadge userBadge, CancellationToken ct = default)
    {
        this.context.UserBadges.Add(userBadge);
        return await this.context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> UpdateAsync(UserBadge userBadge, CancellationToken ct = default)
    {
        this.context.UserBadges.Update(userBadge);
        return await this.context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> DeleteAsync(int userId, int badgeId, CancellationToken ct = default)
    {
        var ub = await this.GetByIdAsync(userId, badgeId, ct);
        if (ub == null) return false;
        this.context.UserBadges.Remove(ub);
        return await this.context.SaveChangesAsync(ct) > 0;
    }
}

