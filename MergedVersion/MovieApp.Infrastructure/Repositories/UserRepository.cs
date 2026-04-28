using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly MovieAppDbContext context;

    public UserRepository(MovieAppDbContext context) => this.context = context;

    public async Task<User?> FindByAuthIdentityAsync(string provider, string subject, CancellationToken ct = default)
    {
        return await this.context.Users
            .FirstOrDefaultAsync(u => u.AuthProvider == provider && u.AuthSubject == subject, ct);
    }

    public async Task<List<User>> GetAllAsync(CancellationToken ct = default)
        => await this.context.Users.ToListAsync(ct);

    public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
        => await this.context.Users
            .Include(u => u.UserStats)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<int> InsertAsync(User user, CancellationToken ct = default)
    {
        this.context.Users.Add(user);
        await this.context.SaveChangesAsync(ct);
        return user.Id;
    }

    public async Task<bool> UpdateAsync(User user, CancellationToken ct = default)
    {
        this.context.Users.Update(user);
        return await this.context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var user = await this.GetByIdAsync(id, ct);
        if (user == null) return false;
        this.context.Users.Remove(user);
        return await this.context.SaveChangesAsync(ct) > 0;
    }
}

