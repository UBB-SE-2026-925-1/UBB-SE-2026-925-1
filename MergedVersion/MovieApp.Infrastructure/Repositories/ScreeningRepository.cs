using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure.Repositories;

public sealed class ScreeningRepository : IScreeningRepository
{
    private readonly MovieAppDbContext context;

    public ScreeningRepository(MovieAppDbContext context) => this.context = context;

    public async Task<IReadOnlyList<Screening>> GetByEventIdAsync(int eventId, CancellationToken ct = default)
        => await this.context.Screenings
            .Where(s => s.EventId == eventId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Screening>> GetByMovieIdAsync(int movieId, CancellationToken ct = default)
        => await this.context.Screenings
            .Where(s => s.MovieId == movieId)
            .ToListAsync(ct);

    public async Task AddAsync(Screening screening, CancellationToken ct = default)
    {
        this.context.Screenings.Add(screening);
        await this.context.SaveChangesAsync(ct);
    }
}


