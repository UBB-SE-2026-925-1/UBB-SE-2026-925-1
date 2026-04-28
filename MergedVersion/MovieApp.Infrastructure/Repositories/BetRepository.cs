using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Interfaces.Repository;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Repositories;

public sealed class BetRepository : IBetRepository
{
    private readonly MovieAppDbContext context;

    public BetRepository(MovieAppDbContext context) => this.context = context;

    public async Task<List<Bet>> GetAllAsync(CancellationToken ct = default)
        => await this.context.Bets
            .Include(b => b.Movie)
            .ToListAsync(ct);

    public async Task<Bet?> GetByIdAsync(int userId, int battleId, CancellationToken ct = default)
        => await this.context.Bets
            .Include(b => b.Movie)
            .FirstOrDefaultAsync(b => b.User.Id == userId && b.Battle.BattleId == battleId, ct);

    public async Task<bool> InsertAsync(Bet bet, CancellationToken ct = default)
    {
        this.context.Bets.Add(bet);
        return await this.context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> UpdateAsync(Bet bet, CancellationToken ct = default)
    {
        this.context.Bets.Update(bet);
        return await this.context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> DeleteAsync(int userId, int battleId, CancellationToken ct = default)
    {
        var bet = await this.GetByIdAsync(userId, battleId, ct);
        if (bet == null) return false;
        this.context.Bets.Remove(bet);
        return await this.context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> DeleteByBattleIdAsync(int battleId, CancellationToken ct = default)
    {
        var battleBets = this.context.Bets.Where(b => b.Battle.BattleId == battleId);
        this.context.Bets.RemoveRange(battleBets);
        return await this.context.SaveChangesAsync(ct) > 0;
    }
}

