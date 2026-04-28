using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure.Repositories;

/// <summary>
/// Provides EF Core implementation for managing user slot machine and spin state.
/// </summary>
public sealed class UserSlotMachineStateRepository : IUserSlotMachineStateRepository
{
    private readonly MovieAppDbContext context;

    public UserSlotMachineStateRepository(MovieAppDbContext context)
    {
        this.context = context;
    }

    /// <inheritdoc/>
    public async Task<UserSpinData?> GetByUserIdAsync(int userIdentifier, CancellationToken ct = default)
    {
        return await this.context.Set<UserSpinData>()
            .FirstOrDefaultAsync(usd => usd.UserId == userIdentifier, ct);
    }

    /// <inheritdoc/>
    public async Task CreateAsync(UserSpinData userSpinData, CancellationToken ct = default)
    {
        this.context.Set<UserSpinData>().Add(userSpinData);
        await this.context.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(UserSpinData userSpinData, CancellationToken ct = default)
    {
        this.context.Set<UserSpinData>().Update(userSpinData);
        await this.context.SaveChangesAsync(ct);
    }
}

