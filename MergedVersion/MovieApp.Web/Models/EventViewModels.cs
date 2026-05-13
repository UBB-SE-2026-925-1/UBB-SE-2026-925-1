using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MovieApp.Core.Models;

namespace MovieApp.Web.Models;

public sealed class EventListViewModel
{
    public IReadOnlyList<Event> Events { get; init; } = Array.Empty<Event>();
    public IReadOnlyCollection<int> JoinedEventIds { get; init; } = Array.Empty<int>();
    public string? SearchQuery { get; init; }
    public string SortKey { get; init; } = EventSortOptions.DateAscending;
    public IReadOnlyList<EventSortOption> SortOptions { get; init; } = EventSortOptions.All;

    public bool HasJoined(int eventId) => this.JoinedEventIds.Contains(eventId);
}

public sealed class EventSortOption
{
    public required string Key { get; init; }
    public required string Label { get; init; }
}

public static class EventSortOptions
{
    public const string DateAscending = "date-asc";
    public const string DateDescending = "date-desc";
    public const string NameAscending = "name-asc";
    public const string CapacityRemainingDesc = "capacity-desc";

    public static readonly IReadOnlyList<EventSortOption> All = new[]
    {
        new EventSortOption { Key = DateAscending, Label = "Date (soonest first)" },
        new EventSortOption { Key = DateDescending, Label = "Date (latest first)" },
        new EventSortOption { Key = NameAscending, Label = "Name (A → Z)" },
        new EventSortOption { Key = CapacityRemainingDesc, Label = "Most spots remaining" },
    };
}

public sealed class EventDetailsViewModel
{
    public required Event Event { get; init; }
    public required IReadOnlyList<EventScreeningRow> Screenings { get; init; }
    public required bool IsJoined { get; init; }
    public required bool IsFull { get; init; }
    public required bool IsPast { get; init; }
    public string? StatusMessage { get; init; }

    public int RemainingSpots => Math.Max(0, this.Event.MaxCapacity - this.Event.CurrentEnrollment);
    public bool CanJoin => !this.IsPast && !this.IsFull && !this.IsJoined;
    public bool CanLeave => !this.IsPast && this.IsJoined;
}

public sealed class EventScreeningRow
{
    public required Screening Screening { get; init; }
    public Movie? Movie { get; init; }

    public string DisplayTitle => this.Movie?.Title ?? $"Movie #{this.Screening.MovieId}";
    public string? PosterUrl => string.IsNullOrWhiteSpace(this.Movie?.PosterUrl) ? null : this.Movie.PosterUrl;
}

public sealed class MyEventsViewModel
{
    public IReadOnlyList<Event> UpcomingEvents { get; init; } = Array.Empty<Event>();
    public IReadOnlyList<Event> PastEvents { get; init; } = Array.Empty<Event>();
}

public sealed class EventCreateInputModel
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, MinimumLength = 5, ErrorMessage = "Description must be between 5 and 2000 characters.")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Url(ErrorMessage = "Poster URL must be a valid absolute URL.")]
    [StringLength(1000)]
    [Display(Name = "Poster URL")]
    public string? PosterUrl { get; set; }

    [Required(ErrorMessage = "Event date/time is required.")]
    [DataType(DataType.DateTime)]
    [Display(Name = "Event date & time")]
    public DateTime EventDateTime { get; set; } = DateTime.Now.AddDays(1).Date.AddHours(19);

    [Required(ErrorMessage = "Location is required.")]
    [StringLength(300, MinimumLength = 2, ErrorMessage = "Location must be between 2 and 300 characters.")]
    [Display(Name = "Location")]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ticket price is required.")]
    [Range(0, 10000, ErrorMessage = "Ticket price must be between 0 and 10000.")]
    [DataType(DataType.Currency)]
    [Display(Name = "Ticket price")]
    public decimal TicketPrice { get; set; }

    [Required(ErrorMessage = "Maximum capacity is required.")]
    [Range(1, 10000, ErrorMessage = "Maximum capacity must be between 1 and 10000.")]
    [Display(Name = "Max capacity")]
    public int MaxCapacity { get; set; } = Event.DefaultMaxCapacity;

    [Required(ErrorMessage = "Event type is required.")]
    [Display(Name = "Event type")]
    public string EventType { get; set; } = string.Empty;

    public static readonly IReadOnlyList<string> EventTypeChoices = new[]
    {
        "Premiere",
        "Marathon",
        "Festival",
        "Q&A",
        "Special Screening",
    };
}
