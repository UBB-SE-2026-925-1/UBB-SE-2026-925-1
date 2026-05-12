using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Proxy;

public class RemoteScreeningRepository : IScreeningRepository
{
    private readonly ApiClient apiClient;

    public RemoteScreeningRepository(ApiClient apiClient) => this.apiClient = apiClient;

    public async Task<Screening?> GetByIdAsync(int screeningId, CancellationToken ct = default) =>
        await this.apiClient.GetAsync<Screening>($"api/screenings/{screeningId}", ct);

    public async Task<IReadOnlyList<Screening>> GetByEventIdAsync(int eventId, CancellationToken ct = default)
    {
        var result = await this.apiClient.GetAsync<IEnumerable<Screening>>($"api/screenings/event/{eventId}", ct);
        return (result?.ToList() ?? new List<Screening>()).AsReadOnly();
    }

    public async Task<IReadOnlyList<Screening>> GetByMovieIdAsync(int movieId, CancellationToken ct = default)
    {
        var result = await this.apiClient.GetAsync<IEnumerable<Screening>>($"api/screenings/movie/{movieId}", ct);
        return (result?.ToList() ?? new List<Screening>()).AsReadOnly();
    }

    public Task AddAsync(Screening screening, CancellationToken ct = default) =>
        this.apiClient.PostAsync<Screening>("api/screenings", screening, ct);
}