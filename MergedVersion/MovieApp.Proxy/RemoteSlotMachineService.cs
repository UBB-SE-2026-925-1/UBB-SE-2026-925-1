using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.Proxy;

public class RemoteSlotMachineService : ISlotMachineService
{
    private readonly ApiClient apiClient;

    public RemoteSlotMachineService(ApiClient apiClient) => this.apiClient = apiClient;

    public async Task<SlotMachineResult> SpinAsync(int userIdentifier)
        => await this.apiClient.PostAsync<object, SlotMachineResult>(
               $"api/slotmachine/spin/{userIdentifier}", new { })
           ?? throw new InvalidOperationException("Spin returned no result.");

    public async Task<int> GetAvailableSpinsAsync(int userIdentifier)
        => await this.apiClient.GetAsync<int>($"api/slotmachine/available/{userIdentifier}");

    public async Task<UserSpinData> GetUserSpinStateAsync(int userIdentifier)
        => await this.apiClient.GetAsync<UserSpinData>($"api/slotmachine/state/{userIdentifier}")
           ?? throw new InvalidOperationException("Could not fetch spin state.");

    public async Task<bool> GrantBonusSpinForEventParticipationAsync(int userIdentifier)
        => await this.apiClient.PostAsync<object, bool>(
               $"api/slotmachine/bonus/{userIdentifier}", new { });

    public async Task<bool> RecordLoginAndCheckStreakAsync(int userIdentifier)
        => await this.apiClient.PostAsync<object, bool>(
               $"api/slotmachine/login-streak/{userIdentifier}", new { });

    public async Task<bool> GrantStreakSpinAsync(int userIdentifier)
        => await this.apiClient.PostAsync<object, bool>(
               $"api/slotmachine/streak-spin/{userIdentifier}", new { });

    public async Task<Genre> GetRandomGenreAsync(CancellationToken cancellationToken = default)
        => await this.apiClient.GetAsync<Genre>("api/slotmachine/reels/genres/random", cancellationToken)
           ?? throw new InvalidOperationException("No genres available.");

    public async Task<Actor> GetRandomActorAsync(CancellationToken cancellationToken = default)
        => await this.apiClient.GetAsync<Actor>("api/slotmachine/reels/actors/random", cancellationToken)
           ?? throw new InvalidOperationException("No actors available.");

    public async Task<Director> GetRandomDirectorAsync(CancellationToken cancellationToken = default)
        => await this.apiClient.GetAsync<Director>("api/slotmachine/reels/directors/random", cancellationToken)
           ?? throw new InvalidOperationException("No directors available.");

    public async Task<IReadOnlyList<Genre>> GetGenresAsync(CancellationToken cancellationToken = default)
        => (await this.apiClient.GetAsync<IEnumerable<Genre>>("api/slotmachine/reels/genres", cancellationToken))
               ?.ToList().AsReadOnly()
           ?? new List<Genre>().AsReadOnly();

    public async Task<IReadOnlyList<Actor>> GetActorsAsync(CancellationToken cancellationToken = default)
        => (await this.apiClient.GetAsync<IEnumerable<Actor>>("api/slotmachine/reels/actors", cancellationToken))
               ?.ToList().AsReadOnly()
           ?? new List<Actor>().AsReadOnly();

    public async Task<IReadOnlyList<Director>> GetDirectorsAsync(CancellationToken cancellationToken = default)
        => (await this.apiClient.GetAsync<IEnumerable<Director>>("api/slotmachine/reels/directors", cancellationToken))
               ?.ToList().AsReadOnly()
           ?? new List<Director>().AsReadOnly();

    public Task<IReadOnlyList<Event>> GetMatchingEventsAsync(int genreIdentifier, int actorIdentifier, int directorIdentifier)
        => throw new NotSupportedException();

    public Task<Movie?> FindJackpotMovieAsync(int genreIdentifier, int actorIdentifier, int directorIdentifier)
        => throw new NotSupportedException();

    public Task GrantJackpotDiscount(int userIdentifier, int movieIdentifier)
        => throw new NotSupportedException();
}
