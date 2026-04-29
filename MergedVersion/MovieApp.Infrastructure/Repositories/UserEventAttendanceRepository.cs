using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure.Repositories;

public sealed class UserEventAttendanceRepository : IUserEventAttendanceRepository
{
    private readonly MovieAppDbContext context;

    public UserEventAttendanceRepository(MovieAppDbContext context) => this.context = context;

    public async Task<IReadOnlyList<int>> GetJoinedEventIdsAsync(int userIdentifier, CancellationToken ct = default)
    {
        return await this.context.Set<UserEventAttendance>()
            .Where(a => a.UserId == userIdentifier)
            .Select(a => a.EventId)
            .ToListAsync(ct);
    }

    public async Task JoinAsync(int userIdentifier, int eventIdentifier, CancellationToken ct = default)
    {
        var attendance = new UserEventAttendance 
        { 
            UserId = userIdentifier, 
            EventId = eventIdentifier,
            JoinedAt = DateTime.UtcNow 
        };
        this.context.Set<UserEventAttendance>().Add(attendance);
        await this.context.SaveChangesAsync(ct);
    }

    public async Task CancelAttendanceAsync(int userIdentifier, int eventIdentifier, CancellationToken ct = default)
    {
        var attendance = await this.context.Set<UserEventAttendance>()
            .FirstOrDefaultAsync(a => a.UserId == userIdentifier && a.EventId == eventIdentifier, ct);
        
        if (attendance != null)
        {
            this.context.Set<UserEventAttendance>().Remove(attendance);
            await this.context.SaveChangesAsync(ct);
        }
    }
}


