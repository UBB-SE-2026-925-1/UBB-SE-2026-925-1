using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using MovieApp.Web.Models;

namespace MovieApp.Web.Controllers;

public class EventsController : Controller
{
    private readonly IEventRepository eventRepository;
    private readonly IUserEventAttendanceRepository attendanceRepository;
    private readonly IScreeningRepository screeningRepository;
    private readonly ICurrentUserService currentUserService;
    private readonly ICatalogService catalogService;

    public EventsController(
        IEventRepository eventRepository,
        IUserEventAttendanceRepository attendanceRepository,
        IScreeningRepository screeningRepository,
        ICurrentUserService currentUserService,
        ICatalogService catalogService)
    {
        this.eventRepository = eventRepository;
        this.attendanceRepository = attendanceRepository;
        this.screeningRepository = screeningRepository;
        this.currentUserService = currentUserService;
        this.catalogService = catalogService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q = null, string? sort = null)
    {
        var all = await this.eventRepository.GetAllAsync();
        var joinedIds = await this.GetJoinedEventIdsSafelyAsync();

        var query = (all ?? Enumerable.Empty<Event>()).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            string needle = q.Trim();
            query = query.Where(e =>
                (e.Title ?? string.Empty).Contains(needle, StringComparison.OrdinalIgnoreCase)
                || (e.LocationReference ?? string.Empty).Contains(needle, StringComparison.OrdinalIgnoreCase));
        }

        string sortKey = NormalizeSort(sort);
        query = sortKey switch
        {
            EventSortOptions.DateDescending => query.OrderByDescending(e => e.EventDateTime),
            EventSortOptions.NameAscending => query.OrderBy(e => e.Title, StringComparer.OrdinalIgnoreCase),
            EventSortOptions.CapacityRemainingDesc => query
                .OrderByDescending(e => Math.Max(0, e.MaxCapacity - e.CurrentEnrollment))
                .ThenBy(e => e.EventDateTime),
            _ => query.OrderBy(e => e.EventDateTime),
        };

        var ordered = query.ToList();
        foreach (var ev in ordered)
        {
            ev.IsJoined = joinedIds.Contains(ev.Id);
        }

        var vm = new EventListViewModel
        {
            Events = ordered,
            JoinedEventIds = joinedIds,
            SearchQuery = q,
            SortKey = sortKey,
        };

        return View(vm);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new EventCreateInputModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventCreateInputModel input)
    {
        if (input.EventDateTime <= DateTime.Now)
        {
            this.ModelState.AddModelError(nameof(EventCreateInputModel.EventDateTime),
                "Event date/time must be in the future.");
        }

        if (!EventCreateInputModel.EventTypeChoices.Contains(input.EventType))
        {
            this.ModelState.AddModelError(nameof(EventCreateInputModel.EventType),
                "Please pick a valid event type from the list.");
        }

        if (!this.ModelState.IsValid)
        {
            return View(input);
        }

        int creatorId;
        try
        {
            await this.currentUserService.InitializeAsync();
            creatorId = this.currentUserService.CurrentUser.Id;
        }
        catch (Exception ex)
        {
            this.ModelState.AddModelError(string.Empty,
                $"Could not resolve the current user: {ex.Message}");
            return View(input);
        }

        var entity = new Event
        {
            Id = 0,
            Title = input.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim(),
            PosterUrl = input.PosterUrl?.Trim() ?? string.Empty,
            EventDateTime = input.EventDateTime,
            LocationReference = input.Location.Trim(),
            TicketPrice = input.TicketPrice,
            MaxCapacity = input.MaxCapacity,
            CurrentEnrollment = 0,
            EventType = input.EventType,
            CreatorUserId = creatorId,
        };

        int newId;
        try
        {
            newId = await this.eventRepository.AddAsync(entity);
        }
        catch (Exception ex)
        {
            this.ModelState.AddModelError(string.Empty,
                $"Could not create the event: {ex.Message}");
            return View(input);
        }

        this.TempData["StatusMessage"] = $"Event \"{entity.Title}\" created.";

        if (newId > 0)
        {
            return RedirectToAction(nameof(Details), new { id = newId });
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var ev = await this.eventRepository.FindByIdAsync(id);
        if (ev is null)
        {
            return NotFound();
        }

        var joinedIds = await this.GetJoinedEventIdsSafelyAsync();
        bool isJoined = joinedIds.Contains(ev.Id);
        ev.IsJoined = isJoined;

        IReadOnlyList<EventScreeningRow> screenings;
        try
        {
            var screeningsRaw = await this.screeningRepository.GetByEventIdAsync(ev.Id);
            var ordered = screeningsRaw.OrderBy(s => s.ScreeningTime).ToList();

            Dictionary<int, Movie> movieLookup;
            try
            {
                var distinctMovieIds = ordered.Select(s => s.MovieId).Distinct().ToHashSet();
                if (distinctMovieIds.Count == 0)
                {
                    movieLookup = new Dictionary<int, Movie>();
                }
                else
                {
                    var allMovies = await this.catalogService.GetAllMoviesAsync();
                    movieLookup = (allMovies ?? new List<Movie>())
                        .Where(m => distinctMovieIds.Contains(m.Id))
                        .GroupBy(m => m.Id)
                        .ToDictionary(g => g.Key, g => g.First());
                }
            }
            catch
            {
                movieLookup = new Dictionary<int, Movie>();
            }

            screenings = ordered
                .Select(s => new EventScreeningRow
                {
                    Screening = s,
                    Movie = movieLookup.TryGetValue(s.MovieId, out var movie) ? movie : null,
                })
                .ToList();
        }
        catch
        {
            screenings = Array.Empty<EventScreeningRow>();
        }

        var vm = new EventDetailsViewModel
        {
            Event = ev,
            Screenings = screenings,
            IsJoined = isJoined,
            IsFull = ev.CurrentEnrollment >= ev.MaxCapacity,
            IsPast = ev.EventDateTime <= DateTime.Now,
            StatusMessage = this.TempData["StatusMessage"] as string,
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Join(int id)
    {
        var ev = await this.eventRepository.FindByIdAsync(id);
        if (ev is null)
        {
            return NotFound();
        }

        int userId;
        try
        {
            await this.currentUserService.InitializeAsync();
            userId = this.currentUserService.CurrentUser.Id;
        }
        catch (Exception ex)
        {
            this.TempData["StatusMessage"] = $"Could not resolve the current user: {ex.Message}";
            return RedirectToAction(nameof(Details), new { id });
        }

        IReadOnlyList<int> joinedIds = await this.GetJoinedEventIdsSafelyAsync(userId);
        bool alreadyJoined = joinedIds.Contains(ev.Id);

        try
        {
            if (alreadyJoined)
            {
                await this.attendanceRepository.CancelAttendanceAsync(userId, ev.Id);
                this.TempData["StatusMessage"] = $"You left \"{ev.Title}\".";
            }
            else
            {
                if (ev.EventDateTime <= DateTime.Now)
                {
                    this.TempData["StatusMessage"] = "This event has already occurred.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                if (ev.CurrentEnrollment >= ev.MaxCapacity)
                {
                    this.TempData["StatusMessage"] = "This event is already full.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                await this.attendanceRepository.JoinAsync(userId, ev.Id);
                this.TempData["StatusMessage"] = $"You joined \"{ev.Title}\".";
            }
        }
        catch (Exception ex)
        {
            this.TempData["StatusMessage"] = $"Could not update attendance: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> MyEvents()
    {
        var joinedIds = await this.GetJoinedEventIdsSafelyAsync();
        if (joinedIds.Count == 0)
        {
            return View(new MyEventsViewModel());
        }

        var all = await this.eventRepository.GetAllAsync();
        var mine = (all ?? Enumerable.Empty<Event>())
            .Where(e => joinedIds.Contains(e.Id))
            .ToList();

        foreach (var ev in mine)
        {
            ev.IsJoined = true;
        }

        var now = DateTime.Now;
        var vm = new MyEventsViewModel
        {
            UpcomingEvents = mine.Where(e => e.EventDateTime > now).OrderBy(e => e.EventDateTime).ToList(),
            PastEvents = mine.Where(e => e.EventDateTime <= now).OrderByDescending(e => e.EventDateTime).ToList(),
        };

        return View(vm);
    }

    private async Task<IReadOnlyList<int>> GetJoinedEventIdsSafelyAsync(int? overrideUserId = null)
    {
        try
        {
            int userId;
            if (overrideUserId.HasValue)
            {
                userId = overrideUserId.Value;
            }
            else
            {
                await this.currentUserService.InitializeAsync();
                userId = this.currentUserService.CurrentUser.Id;
            }

            return await this.attendanceRepository.GetJoinedEventIdsAsync(userId);
        }
        catch
        {
            return Array.Empty<int>();
        }
    }

    private static string NormalizeSort(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return EventSortOptions.DateAscending;
        }

        return EventSortOptions.All.Any(o => o.Key == sort)
            ? sort
            : EventSortOptions.DateAscending;
    }
}
