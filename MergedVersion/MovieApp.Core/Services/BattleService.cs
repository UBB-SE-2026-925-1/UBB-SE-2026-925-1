using System.Threading;
using System.Threading.Tasks;
#nullable enable

using MovieApp.Core.Interfaces.Repository;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Core.Services;

/// <summary>
/// Provides logic for managing movie battles and processing user bets.
/// </summary>
public sealed class BattleService : IBattleService
{
    private readonly IBattleRepository battleRepository;
    private readonly IBetRepository betRepository;
    private readonly IMovieRepository movieRepository;
    private readonly IUserRepository userRepository;
    private readonly IPointService pointService;

    public BattleService(
        IBattleRepository battleRepository,
        IBetRepository betRepository,
        IMovieRepository movieRepository,
        IUserRepository userRepository,
        IPointService pointService)
    {
        this.battleRepository = battleRepository;
        this.betRepository = betRepository;
        this.movieRepository = movieRepository;
        this.userRepository = userRepository;
        this.pointService = pointService;
    }

    /// <inheritdoc/>
    public async Task<Battle?> GetActiveBattleAsync(CancellationToken ct = default)
    {
        var battles = await this.battleRepository.GetAllAsync(ct);
        var active = battles.FirstOrDefault(b => b.Status == "Active");

        if (active != null)
        {
            active.FirstMovie = await this.movieRepository.GetByIdAsync(active.FirstMovie?.Id ?? 0, ct) ?? active.FirstMovie;
            active.SecondMovie = await this.movieRepository.GetByIdAsync(active.SecondMovie?.Id ?? 0, ct) ?? active.SecondMovie;
        }

        return active;
    }

    /// <inheritdoc/>
    public async Task<Battle> CreateBattleAsync(int firstMovieId, int secondMovieId, CancellationToken ct = default)
    {
        var allBattles = await this.battleRepository.GetAllAsync(ct);
        if (allBattles.Any(b => b.Status == "Active"))
        {
            throw new InvalidOperationException("An active battle already exists.");
        }

        var first = await this.movieRepository.GetByIdAsync(firstMovieId, ct) ?? throw new InvalidOperationException("First movie not found.");
        var second = await this.movieRepository.GetByIdAsync(secondMovieId, ct) ?? throw new InvalidOperationException("Second movie not found.");

        // Rating difference check removed to allow more matches
        
        var startDate = DateTime.UtcNow.Date; // Logic simplified for demo; usually Monday
        var battle = new Battle
        {
            FirstMovie = first,
            SecondMovie = second,
            InitialRatingFirstMovie = first.AverageRating,
            InitialRatingSecondMovie = second.AverageRating,
            StartDate = startDate,
            EndDate = startDate.AddDays(6),
            Status = "Active"
        };

        await this.battleRepository.InsertAsync(battle, ct);
        return battle;
    }

    /// <inheritdoc/>
    public async Task<Bet> PlaceBetAsync(int userId, int battleId, int movieId, int amount, CancellationToken ct = default)
    {
        if (amount <= 0) throw new InvalidOperationException("Amount must be positive.");

        var bets = await this.betRepository.GetAllAsync(ct);
        if (bets.Any(b => b.User?.Id == userId && b.Battle?.BattleId == battleId))
        {
            throw new InvalidOperationException("User has already bet.");
        }

        var user = await this.userRepository.GetByIdAsync(userId, ct) ?? throw new InvalidOperationException("User not found.");
        var battle = await this.battleRepository.GetByIdAsync(battleId, ct) ?? throw new InvalidOperationException("Battle not found.");
        var movie = await this.movieRepository.GetByIdAsync(movieId, ct) ?? throw new InvalidOperationException("Movie not found.");

        await this.pointService.FreezePointsAsync(userId, amount, ct);

        var bet = new Bet { User = user, Battle = battle, Movie = movie, Amount = amount };
        await this.betRepository.InsertAsync(bet, ct);
        return bet;
    }

    /// <inheritdoc/>
    public async Task<int> DetermineWinnerAsync(int battleId, CancellationToken ct = default)
    {
        var battle = await this.battleRepository.GetByIdAsync(battleId, ct) ?? throw new InvalidOperationException("Battle not found.");

        var m1 = await this.movieRepository.GetByIdAsync(battle.FirstMovie?.Id ?? 0, ct);
        var m2 = await this.movieRepository.GetByIdAsync(battle.SecondMovie?.Id ?? 0, ct);

        double growth1 = (m1?.AverageRating ?? 0) - battle.InitialRatingFirstMovie;
        double growth2 = (m2?.AverageRating ?? 0) - battle.InitialRatingSecondMovie;

        return growth1 >= growth2 ? (m1?.Id ?? 0) : (m2?.Id ?? 0);
    }

    /// <inheritdoc/>
    public async Task DistributePayoutsAsync(int battleId, CancellationToken ct = default)
    {
        int winnerId = await this.DetermineWinnerAsync(battleId, ct);
        var bets = await this.betRepository.GetAllAsync(ct);
        var battleBets = bets.Where(b => b.Battle?.BattleId == battleId).ToList();

        foreach (var bet in battleBets)
        {
            if (bet.Movie?.Id == winnerId)
            {
                await this.pointService.RefundPointsAsync(bet.User?.Id ?? 0, bet.Amount * 2, ct);
            }
        }

        var battle = await this.battleRepository.GetByIdAsync(battleId, ct);
        if (battle != null)
        {
            battle.Status = "Finished";
            await this.battleRepository.UpdateAsync(battle, ct);
        }
    }

    /// <inheritdoc/>
    public async Task SettleExpiredBattlesAsync(CancellationToken ct = default)
    {
        var battles = await this.battleRepository.GetAllAsync(ct);
        var expired = battles.Where(b => b.Status == "Active" && b.EndDate < DateTime.UtcNow.Date);

        foreach (var battle in expired)
        {
            await this.DistributePayoutsAsync(battle.BattleId, ct);
        }
    }

    // --- Demo Helpers ---

    public async Task ForceSettleBattleAsync(int battleId, CancellationToken ct = default)
    {
        await this.DistributePayoutsAsync(battleId, ct);
    }

    public async Task ResetAllBattlesForDemoAsync(CancellationToken ct = default)
    {
        var battles = await this.battleRepository.GetAllAsync(ct);
        foreach (var b in battles) await this.battleRepository.DeleteAsync(b.BattleId, ct);
    }

    public async Task<Battle> CreateDemoBattleAsync(CancellationToken ct = default)
    {
        var movies = await this.movieRepository.GetAllAsync(ct);
        if (movies.Count < 2) throw new InvalidOperationException("Not enough movies for a battle.");

        // Find the two movies with the closest ratings for a fair fight
        var sortedMovies = movies.OrderBy(m => m.AverageRating).ToList();
        Movie bestM1 = sortedMovies[0];
        Movie bestM2 = sortedMovies[1];
        double minDiff = Math.Abs(bestM1.AverageRating - bestM2.AverageRating);

        for (int i = 0; i < sortedMovies.Count - 1; i++)
        {
            double diff = Math.Abs(sortedMovies[i].AverageRating - sortedMovies[i + 1].AverageRating);
            if (diff < minDiff)
            {
                minDiff = diff;
                bestM1 = sortedMovies[i];
                bestM2 = sortedMovies[i + 1];
            }
        }

        return await this.CreateBattleAsync(bestM1.Id, bestM2.Id, ct);
    }

    public async Task<Battle?> GetCurrentBattleForUserAsync(int userId, CancellationToken ct = default)
    {
        return await this.GetActiveBattleAsync(ct);
    }

    public async Task<Bet?> GetBetAsync(int userId, int battleId, CancellationToken ct = default)
    {
        var bets = await this.betRepository.GetAllAsync(ct);
        return bets.FirstOrDefault(b => b.User?.Id == userId && b.Battle?.BattleId == battleId);
    }
}

