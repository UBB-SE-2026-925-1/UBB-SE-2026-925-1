using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly MovieAppDbContext context;

    public NotificationRepository(MovieAppDbContext context) => this.context = context;

    public async Task AddAsync(Notification notification, CancellationToken ct = default)
    {
        this.context.Notifications.Add(notification);
        await this.context.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(int notificationId, CancellationToken ct = default)
    {
        var notification = await this.context.Notifications.FindAsync(new object[] { notificationId }, ct);
        if (notification != null)
        {
            this.context.Notifications.Remove(notification);
            await this.context.SaveChangesAsync(ct);
        }
    }

    public async Task<IReadOnlyList<Notification>> FindByUserAsync(int userId, CancellationToken ct = default)
    {
        return await this.context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }
}


