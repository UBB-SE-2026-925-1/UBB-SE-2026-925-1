using MovieApp.Core.Models;

namespace MovieApp.Web.Models;

public sealed class MarathonListItemViewModel
{
    public required Marathon Marathon { get; init; }
    public int TotalMovies { get; init; }
    public int CompletedMovies { get; init; }
    public int ParticipantCount { get; init; }
    public bool IsJoined { get; init; }

    public int ProgressPercent => this.TotalMovies <= 0
        ? 0
        : (int)Math.Round(100.0 * this.CompletedMovies / this.TotalMovies);
}

public sealed class MarathonIndexViewModel
{
    public IReadOnlyList<MarathonListItemViewModel> Items { get; init; } = Array.Empty<MarathonListItemViewModel>();
    public string? StatusMessage { get; init; }
}

public sealed class MarathonDetailViewModel
{
    public required Marathon Marathon { get; init; }
    public IReadOnlyList<Movie> Movies { get; init; } = Array.Empty<Movie>();
    public IReadOnlyList<LeaderboardEntry> Leaderboard { get; init; } = Array.Empty<LeaderboardEntry>();
    public MarathonProgress? Progress { get; init; }
    public bool IsJoined => this.Progress is not null;
    public bool IsLocked { get; init; }
    public int CurrentUserId { get; init; }
    public string? StatusMessage { get; init; }

    public int CompletedMovies => this.Progress?.CompletedMoviesCount ?? 0;
    public int TotalMovies => this.Movies.Count;
    public int ProgressPercent => this.TotalMovies <= 0
        ? 0
        : (int)Math.Round(100.0 * this.CompletedMovies / this.TotalMovies);
}

public sealed class MarathonLeaderboardViewModel
{
    public required Marathon Marathon { get; init; }
    public IReadOnlyList<LeaderboardEntry> Leaderboard { get; init; } = Array.Empty<LeaderboardEntry>();
}
