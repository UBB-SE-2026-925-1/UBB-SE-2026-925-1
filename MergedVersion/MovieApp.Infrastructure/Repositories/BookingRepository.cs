using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure.Repositories;

public sealed class BookingRepository : IBookingRepository
{
    private readonly MovieAppDbContext context;

    public BookingRepository(MovieAppDbContext context) => this.context = context;

    public async Task<IReadOnlyList<SeatBooking>> GetByScreeningAsync(int screeningId, CancellationToken ct = default)
        => await this.context.SeatBookings
            .Where(b => b.ScreeningId == screeningId)
            .ToListAsync(ct);

    public async Task<bool> ReserveAsync(int screeningId, int userId, IReadOnlyList<(int Row, int Column)> seats, CancellationToken ct = default)
    {
        if (seats is null || seats.Count == 0)
        {
            return false;
        }

        await using var tx = await this.context.Database.BeginTransactionAsync(ct);

        var rows = seats.Select(s => s.Row).ToList();
        var cols = seats.Select(s => s.Column).ToList();

        var existing = await this.context.SeatBookings
            .Where(b => b.ScreeningId == screeningId
                        && rows.Contains(b.Row)
                        && cols.Contains(b.Column))
            .ToListAsync(ct);

        var clash = existing.Any(b => seats.Any(s => s.Row == b.Row && s.Column == b.Column));
        if (clash)
        {
            return false;
        }

        var now = DateTime.UtcNow;
        foreach (var s in seats)
        {
            this.context.SeatBookings.Add(new SeatBooking
            {
                ScreeningId = screeningId,
                UserId = userId,
                Row = s.Row,
                Column = s.Column,
                BookedAt = now,
            });
        }

        await this.context.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return true;
    }
}
