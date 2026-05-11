using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.UI.Services.Api;

public sealed class RemoteBookingRepository : IBookingRepository
{
    private readonly ApiClient apiClient;

    public RemoteBookingRepository(ApiClient apiClient) => this.apiClient = apiClient;

    public async Task<IReadOnlyList<SeatBooking>> GetByScreeningAsync(int screeningId, CancellationToken ct = default)
    {
        var result = await this.apiClient.GetAsync<IEnumerable<SeatBooking>>($"api/bookings/screening/{screeningId}", ct);
        return (result?.ToList() ?? new List<SeatBooking>()).AsReadOnly();
    }

    public async Task<bool> ReserveAsync(int screeningId, int userId, IReadOnlyList<(int Row, int Column)> seats, CancellationToken ct = default)
    {
        var payload = new
        {
            ScreeningId = screeningId,
            UserId = userId,
            Seats = seats.Select(s => new { s.Row, s.Column }).ToList(),
        };

        return await this.apiClient.PostAsync<object, bool>("api/bookings/reserve", payload, ct);
    }
}
