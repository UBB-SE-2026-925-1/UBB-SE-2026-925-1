using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Interfaces.Repository;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure.Repositories;

public sealed class BattleRepository : IBattleRepository
{
    private readonly MovieAppDbContext context;

    public BattleRepository(MovieAppDbContext context) => this.context = context;

    public async Task<List<Battle>> GetAllAsync(CancellationToken ct = default)
        => await this.context.Battles
            .Include(b => b.FirstMovie)
            .Include(b => b.SecondMovie)
            .Include(b => b.Bets)
                .ThenInclude(bet => bet.User)
            .Include(b => b.Bets)
                .ThenInclude(bet => bet.Movie)
            .ToListAsync(ct);

    public async Task<Battle?> GetByIdAsync(int id, CancellationToken ct = default)
        => await this.context.Battles
            .Include(b => b.FirstMovie)
            .Include(b => b.SecondMovie)
            .Include(b => b.Bets)
                .ThenInclude(bet => bet.User)
            .Include(b => b.Bets)
                .ThenInclude(bet => bet.Movie)
            .FirstOrDefaultAsync(b => b.BattleId == id, ct);

    public async Task<int> InsertAsync(Battle battle, CancellationToken ct = default)
    {
        this.context.Battles.Add(battle);
        await this.context.SaveChangesAsync(ct);
        return battle.BattleId;
    }

    public async Task<bool> UpdateAsync(Battle battle, CancellationToken ct = default)
    {
        this.context.Battles.Update(battle);
        return await this.context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var battle = await this.GetByIdAsync(id, ct);
        if (battle == null) return false;
        this.context.Battles.Remove(battle);
        return await this.context.SaveChangesAsync(ct) > 0;
    }
}

