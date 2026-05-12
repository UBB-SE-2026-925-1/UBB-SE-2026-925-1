using System.ComponentModel.DataAnnotations;
using MovieApp.Core.Models;

namespace MovieApp.Web.Models;

public class BattleViewModel
{
    public Battle? Battle { get; set; }

    public Bet? UserBet { get; set; }

    public int? CurrentUserPoints { get; set; }

    public int? WinnerMovieId { get; set; }

    public string? StatusMessage { get; set; }

    public int FirstMovieBetCount => this.GetBetCount(this.Battle?.FirstMovie?.Id);

    public int SecondMovieBetCount => this.GetBetCount(this.Battle?.SecondMovie?.Id);

    public int FirstMoviePointTotal => this.GetPointTotal(this.Battle?.FirstMovie?.Id);

    public int SecondMoviePointTotal => this.GetPointTotal(this.Battle?.SecondMovie?.Id);

    public IReadOnlyList<BattleLeaderboardRow> LeaderboardRows =>
        this.Battle?.Bets
            .GroupBy(b => b.User?.Id ?? 0)
            .Select(g => new BattleLeaderboardRow
            {
                Username = g.First().User?.Username ?? "Unknown user",
                MovieTitle = g.First().Movie?.Title ?? "Unknown movie",
                Amount = g.Sum(b => b.Amount)
            })
            .OrderByDescending(row => row.Amount)
            .ThenBy(row => row.Username)
            .ToList()
        ?? new List<BattleLeaderboardRow>();

    private int GetBetCount(int? movieId)
        => movieId.HasValue && this.Battle != null
            ? this.Battle.Bets.Count(b => b.Movie?.Id == movieId.Value)
            : 0;

    private int GetPointTotal(int? movieId)
        => movieId.HasValue && this.Battle != null
            ? this.Battle.Bets.Where(b => b.Movie?.Id == movieId.Value).Sum(b => b.Amount)
            : 0;
}

public class BattleLeaderboardRow
{
    public string Username { get; set; } = string.Empty;

    public string MovieTitle { get; set; } = string.Empty;

    public int Amount { get; set; }
}

public class PlaceBattleBetInputModel
{
    [Required]
    public int BattleId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Choose a movie.")]
    public int MovieId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Enter a whole number greater than zero.")]
    public int Amount { get; set; }
}
