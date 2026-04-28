using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure.Repositories;

public sealed class TriviaRepository : ITriviaRepository
{
    private readonly MovieAppDbContext context;

    public TriviaRepository(MovieAppDbContext context) => this.context = context;

    public async Task<List<TriviaQuestion>> GetAllAsync(CancellationToken ct = default)
        => await this.context.TriviaQuestions.ToListAsync(ct);

    public async Task<TriviaQuestion?> GetRandomAsync(CancellationToken ct = default)
    {
        return await this.context.TriviaQuestions
            .OrderBy(r => Guid.NewGuid())
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<TriviaQuestion>> GetByCategoryAsync(string categoryName, CancellationToken ct = default)
    {
        return await this.context.TriviaQuestions
            .Where(q => q.Category == categoryName)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<TriviaQuestion>> GetByMovieIdAsync(int movieId, int questionCount = ITriviaRepository.DefaultQuestionCount, CancellationToken ct = default)
    {
        return await this.context.TriviaQuestions
            .Where(q => q.MovieId == movieId)
            .Take(questionCount)
            .ToListAsync(ct);
    }
}

