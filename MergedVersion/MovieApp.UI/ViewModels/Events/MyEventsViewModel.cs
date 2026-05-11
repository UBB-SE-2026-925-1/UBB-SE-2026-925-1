namespace MovieApp.UI.ViewModels.Events;

using System.Collections.ObjectModel;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

/// <summary>
/// Represents the user's personal event workspace.
/// </summary>
/// <remarks>
/// The page shell is in place, but the current implementation still returns an
/// empty list until a backing repository flow is wired in.
/// </remarks>
public sealed class MyEventsViewModel : EventListPageViewModel
{
    private readonly IPriceWatcherRepository priceWatcherRepository;
    private readonly IEventRepository? eventRepository;
    private readonly IUserEventAttendanceRepository? attendanceRepository;
    private HashSet<int> joinedIds = new();
    private WatchedEvent? selectedWatchedEvent;
    private double selectedTargetPrice;

    public MyEventsViewModel(
        IPriceWatcherRepository priceWatcherRepository,
        IEventRepository? eventRepository,
        IUserEventAttendanceRepository? attendanceRepository)
    {
        this.priceWatcherRepository = priceWatcherRepository;
        this.eventRepository = eventRepository;
        this.attendanceRepository = attendanceRepository;

        this.SaveCreatedEventCommand = new AsyncRelayCommand(this.SaveCreatedEventAsync, () => this.SelectedEvent is not null);
        this.CancelParticipationCommand = new AsyncRelayCommand(this.CancelParticipationAsync, () => this.SelectedEvent is not null);
        this.DeleteEventCommand = new AsyncRelayCommand(this.DeleteEventAsync, () => this.SelectedEvent is not null);
    }

    private Event? selectedEvent;
    private string formTitle = string.Empty;
    private string formLocation = string.Empty;
    private double formPrice;
    private int formCapacity;
    private string formNotes = string.Empty;

    public Event? SelectedEvent
    {
        get => this.selectedEvent;
        set
        {
            if (this.SetProperty(ref this.selectedEvent, value))
            {
                if (value != null)
                {
                    this.FormTitle = value.Title;
                    this.FormLocation = value.LocationReference;
                    this.FormPrice = (double)value.TicketPrice;
                    this.FormCapacity = value.MaxCapacity;
                    // Notes would typically be user-specific, for now we just clear it or load from somewhere if available
                }
                ((AsyncRelayCommand)this.SaveCreatedEventCommand).NotifyCanExecuteChanged();
                ((AsyncRelayCommand)this.CancelParticipationCommand).NotifyCanExecuteChanged();
            }
        }
    }

    public string FormTitle { get => formTitle; set => SetProperty(ref formTitle, value); }
    public string FormLocation { get => formLocation; set => SetProperty(ref formLocation, value); }
    public double FormPrice { get => formPrice; set => SetProperty(ref formPrice, value); }
    public int FormCapacity { get => formCapacity; set => SetProperty(ref formCapacity, value); }
    public string FormNotes { get => formNotes; set => SetProperty(ref formNotes, value); }

    public System.Windows.Input.ICommand SaveCreatedEventCommand { get; }
    public System.Windows.Input.ICommand CancelParticipationCommand { get; }
    public System.Windows.Input.ICommand DeleteEventCommand { get; }

    private async Task SaveCreatedEventAsync()
    {
        if (this.SelectedEvent == null || this.eventRepository == null) return;
        
        Event updated = new Event
        {
            Id = this.SelectedEvent.Id,
            Title = this.FormTitle,
            EventDateTime = this.SelectedEvent.EventDateTime,
            CreatorUserId = this.SelectedEvent.CreatorUserId,
            Description = this.SelectedEvent.Description,
            PosterUrl = this.SelectedEvent.PosterUrl,
            LocationReference = this.FormLocation,
            TicketPrice = (decimal)this.FormPrice,
            MaxCapacity = this.FormCapacity,
            EventType = this.SelectedEvent.EventType,
            CurrentEnrollment = this.SelectedEvent.CurrentEnrollment,
            HistoricalRating = this.SelectedEvent.HistoricalRating
        };

        await this.eventRepository.UpdateEventAsync(updated);
        await this.InitializeAsync();
    }

    private async Task CancelParticipationAsync()
    {
        if (this.SelectedEvent == null || this.attendanceRepository == null) return;

        try
        {
            int eventId = this.SelectedEvent.Id;

            await this.attendanceRepository.CancelAttendanceAsync(App.CurrentUserId, eventId);

            if (this.eventRepository is not null)
            {
                var ev = await this.eventRepository.FindByIdAsync(eventId);
                if (ev is not null && ev.CurrentEnrollment > 0)
                {
                    await this.eventRepository.UpdateEnrollmentAsync(eventId, ev.CurrentEnrollment - 1);
                }
            }

            this.joinedIds.Remove(eventId);
            this.SelectedEvent = null;

            await this.InitializeAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CancelParticipation error: {ex.Message}");
        }
    }

    private async Task DeleteEventAsync()
    {
        if (this.SelectedEvent == null || this.eventRepository == null) return;
        
        await this.eventRepository.DeleteAsync(this.SelectedEvent.Id);
        await this.InitializeAsync();
    }

    /// <inheritdoc/>
    public override string PageTitle => "My Events";

    /// <inheritdoc/>
    public override void RefreshVisibleEvents()
    {
        base.RefreshVisibleEvents();
        this.RefreshFilteredCollections();
        this.OnPropertyChanged(nameof(this.VisibleCreatedEvents));
        this.OnPropertyChanged(nameof(this.VisibleJoinedEvents));
    }

    public IEnumerable<Event> VisibleCreatedEvents => 
        this.VisibleEvents.Where(e => e.CreatorUserId == App.CurrentUserId);

    public IEnumerable<Event> VisibleJoinedEvents => 
        this.VisibleEvents.Where(e => this.joinedIds.Contains(e.Id));

    public ObservableCollection<Event> CreatedEvents { get; } = new ();

    public ObservableCollection<Event> JoinedEvents { get; } = new ();

    /// <summary>
    /// Gets the collection of events watched by the user.
    /// </summary>
    public ObservableCollection<WatchedEvent> WatchedEvents { get; } = new ();

    /// <summary>
    /// Gets or sets the currently selected watched event.
    /// </summary>
    public WatchedEvent? SelectedWatchedEvent
    {
        get => this.selectedWatchedEvent;
        set
        {
            if (this.SetProperty(ref this.selectedWatchedEvent, value))
            {
                this.OnPropertyChanged(nameof(this.SelectedEventIdText));
                this.SelectedTargetPrice = value != null ? (double)value.TargetPrice : 0;
            }
        }
    }

    /// <summary>
    /// Gets the string representation of the selected event's identifier.
    /// </summary>
    public string SelectedEventIdText => this.SelectedWatchedEvent?.EventId.ToString() ?? string.Empty;

    /// <summary>
    /// Gets or sets the target price for the selected watched event.
    /// </summary>
    public double SelectedTargetPrice
    {
        get => this.selectedTargetPrice;
        set => this.SetProperty(ref this.selectedTargetPrice, value);
    }

    /// <summary>
    /// Loads the user's watchlist from local storage into the collection.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task LoadWatchlistAsync()
    {
        List<WatchedEvent> items = await priceWatcherRepository.GetAllWatchedEventsAsync();

        this.WatchedEvents.Clear();
        foreach (WatchedEvent item in items)
        {
            this.WatchedEvents.Add(item);
        }
    }

    /// <summary>
    /// Saves the currently selected watched event with the updated target price.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SaveSelectedWatchlistAsync()
    {
        if (this.SelectedWatchedEvent != null)
        {
            this.SelectedWatchedEvent.TargetPrice = (decimal)this.SelectedTargetPrice;

            await priceWatcherRepository.RemoveWatchAsync(this.SelectedWatchedEvent.EventId);
            await priceWatcherRepository.AddWatchAsync(this.SelectedWatchedEvent);

            await this.LoadWatchlistAsync();
        }
    }

    public void RefreshFilteredCollections()
    {
        int userId = App.CurrentUserId;
        
        this.CreatedEvents.Clear();
        foreach (var e in this.AllEvents.Where(e => e.CreatorUserId == userId))
        {
            this.CreatedEvents.Add(e);
        }

        this.JoinedEvents.Clear();
        foreach (var e in this.AllEvents.Where(e => this.joinedIds.Contains(e.Id)))
        {
            this.JoinedEvents.Add(e);
        }
    }

    /// <inheritdoc/>
    protected override async Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        if (this.eventRepository is null || this.attendanceRepository is null)
        {
            return new List<Event>();
        }

        int userId = App.CurrentUserId;

        var ids = await this.attendanceRepository.GetJoinedEventIdsAsync(userId);
        this.joinedIds = new HashSet<int>(ids);

        var allEvents = await this.eventRepository.GetAllAsync();

        return allEvents
            .Where(e => this.joinedIds.Contains(e.Id) || e.CreatorUserId == userId)
            .ToList();
    }

}
