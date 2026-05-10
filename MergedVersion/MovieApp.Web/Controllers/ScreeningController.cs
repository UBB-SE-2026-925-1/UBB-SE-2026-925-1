using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using MovieApp.Proxy;
using MovieApp.Web.Models;

namespace MovieApp.Web.Controllers;

public class ScreeningController : Controller
{
    private readonly IScreeningRepository screeningRepository;
    private readonly IBookingRepository bookingRepository;
    private readonly ICatalogService catalogService;
    private readonly ICurrentUserService currentUserService;
    private readonly ApiClient apiClient;

    public ScreeningController(
        IScreeningRepository screeningRepository,
        IBookingRepository bookingRepository,
        ICatalogService catalogService,
        ICurrentUserService currentUserService,
        ApiClient apiClient)
    {
        this.screeningRepository = screeningRepository;
        this.bookingRepository = bookingRepository;
        this.catalogService = catalogService;
        this.currentUserService = currentUserService;
        this.apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> ForMovie(int id)
    {
        var movie = await this.catalogService.GetMovieByIdAsync(id);
        if (movie is null)
        {
            return NotFound();
        }

        var screenings = await this.screeningRepository.GetByMovieIdAsync(id);
        var ordered = screenings.OrderBy(s => s.ScreeningTime).ToList();

        var vm = new ScreeningListViewModel
        {
            Movie = movie,
            Screenings = ordered,
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Checkout(int id)
    {
        var screening = await this.screeningRepository.GetByIdAsync(id);
        if (screening is null)
        {
            return NotFound();
        }

        return await this.BuildCheckoutViewAsync(screening, statusMessage: null);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reserve(int screeningId, string selectedSeats)
    {
        var screening = await this.screeningRepository.GetByIdAsync(screeningId);
        if (screening is null)
        {
            return NotFound();
        }

        var seats = ParseSeats(selectedSeats);
        if (seats.Count == 0)
        {
            return await this.BuildCheckoutViewAsync(screening, "Please pick at least one seat.");
        }

        await this.currentUserService.InitializeAsync();
        int userId = this.currentUserService.CurrentUser.Id;

        try
        {
            bool ok = await this.bookingRepository.ReserveAsync(screeningId, userId, seats);
            if (!ok)
            {
                return await this.BuildCheckoutViewAsync(screening,
                    "One or more selected seats were just booked by someone else. Please pick again.");
            }

            var en = CultureInfo.GetCultureInfo("en-US");
            TempData["StatusMessage"] = $"Reserved {seats.Count} seat(s) for the screening on {screening.ScreeningTime.ToString("dddd, dd MMMM yyyy 'at' HH:mm", en)}.";
            return RedirectToAction(nameof(ForMovie), new { id = screening.MovieId });
        }
        catch (System.Exception ex)
        {
            return await this.BuildCheckoutViewAsync(screening, $"Reservation failed: {ex.Message}");
        }
    }

    private async Task<IActionResult> BuildCheckoutViewAsync(Screening screening, string? statusMessage)
    {
        var movie = await this.catalogService.GetMovieByIdAsync(screening.MovieId);
        var ev = await this.apiClient.GetAsync<Event>($"api/events/{screening.EventId}");
        var bookings = await this.bookingRepository.GetByScreeningAsync(screening.Id);

        var (totalRows, totalColumns) = RoomLayout.For(screening.Id);
        var bookedSet = new HashSet<(int, int)>(bookings.Select(b => (b.Row, b.Column)));

        var seats = new List<SeatViewModel>();
        for (int row = 1; row <= totalRows; row++)
        {
            for (int col = 1; col <= totalColumns; col++)
            {
                seats.Add(new SeatViewModel
                {
                    Row = row,
                    Column = col,
                    IsBooked = bookedSet.Contains((row, col)),
                });
            }
        }

        var vm = new CheckoutViewModel
        {
            Screening = screening,
            Movie = movie,
            Event = ev,
            Seats = seats,
            TotalRows = totalRows,
            TotalColumns = totalColumns,
            StatusMessage = statusMessage,
        };

        return View("Checkout", vm);
    }

    private static List<(int Row, int Column)> ParseSeats(string? raw)
    {
        var result = new List<(int, int)>();
        if (string.IsNullOrWhiteSpace(raw)) return result;

        foreach (var token in raw.Split(',', System.StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = token.Split('-');
            if (parts.Length == 2
                && int.TryParse(parts[0], out int row)
                && int.TryParse(parts[1], out int col))
            {
                result.Add((row, col));
            }
        }

        return result;
    }
}
