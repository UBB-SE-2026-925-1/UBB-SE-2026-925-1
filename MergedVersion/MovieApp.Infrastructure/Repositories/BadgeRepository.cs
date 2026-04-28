using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Interfaces.Repository;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Repositories;

public sealed class BadgeRepository : IBadgeRepository
{
    private readonly MovieAppDbContext context;

    public BadgeRepository(MovieAppDbContext context) => this.context = context;

    public async Task<List<Badge>> GetAllAsync(CancellationToken ct = default)
        => await this.context.Badges.ToListAsync(ct);

    public async Task<Badge?> GetByIdAsync(int id, CancellationToken ct = default)
        => await this.context.Badges.FindAsync(new object[] { id }, ct);

    public async Task<int> InsertAsync(Badge badge, CancellationToken ct = default)
    {
        this.context.Badges.Add(badge);
        await this.context.SaveChangesAsync(ct);
        return badge.BadgeId;
    }

    public async Task<bool> UpdateAsync(Badge badge, CancellationToken ct = default)
    {
        this.context.Badges.Update(badge);
        return await this.context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var badge = await this.GetByIdAsync(id, ct);
        if (badge == null) return false;
        this.context.Badges.Remove(badge);
        return await this.context.SaveChangesAsync(ct) > 0;
    }
}

