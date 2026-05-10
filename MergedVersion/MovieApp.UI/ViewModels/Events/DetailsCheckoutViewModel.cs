// <copyright file="DetailsCheckoutViewModel.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.UI.ViewModels.Events;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;

/// <summary>
/// Drives the screening selection, seat selection, discount application, and checkout flow
/// for the details/checkout surface.
/// </summary>
public sealed class DetailsCheckoutViewModel : ViewModels.ViewModelBase
{
    private const string SlotMachineRewardType = "MovieDiscount";

    private readonly IScreeningRepository screeningRepository;
    private readonly IEventRepository eventRepository;
    private readonly IUserMovieDiscountRepository userMovieDiscountRepository;
    private readonly IPriceWatcherRepository priceWatcherRepository;
    private readonly IUserEventAttendanceRepository userEventAttendanceRepository;
    private readonly IBookingRepository bookingRepository;
    private readonly IReferralValidator? referralValidator;
    private readonly int currentUserId;
    private int screeningChangeGeneration;
    private IReadOnlyList<(int Row, int Column)> confirmedSelection = Array.Empty<(int, int)>();

    private Movie? movie;
    private Screening? selectedScreening;
    private Event? currentEvent;
    private int selectedSeatCount;
    private decimal basePrice;
    private decimal finalPrice;
    private bool applyFreeEnrollment;
    private bool applyTriviaReward;
    private bool applySlotMachineDiscount;
    private double slotMachineDiscountPercent;
    private bool isWatchingPrice;
    private string statusMessage = string.Empty;
    private bool hasUserJoined;
    private string referralCode = string.Empty;
    private string referralStatus = string.Empty;
    private bool isLoading;
    private bool isPurchaseInFlight;
    private SeatGuideViewModel? seatGuide;

    /// <summary>
    /// Initializes a new instance of the <see cref="DetailsCheckoutViewModel"/> class.
    /// </summary>
    /// <param name="screeningRepository">Provides access to screening data.</param>
    /// <param name="eventRepository">Provides access to event data.</param>
    /// <param name="userMovieDiscountRepository">Provides discount rewards available to the user.</param>
    /// <param name="priceWatcherRepository">Provides price-watch state for events.</param>
    /// <param name="userEventAttendanceRepository">Tracks which events the user is enrolled in.</param>
    /// <param name="referralValidator">Optional referral validator for the referral code box.</param>
    /// <param name="currentUserId">Identifier of the currently signed-in user.</param>
    public DetailsCheckoutViewModel(
        IScreeningRepository screeningRepository,
        IEventRepository eventRepository,
        IUserMovieDiscountRepository userMovieDiscountRepository,
        IPriceWatcherRepository priceWatcherRepository,
        IUserEventAttendanceRepository userEventAttendanceRepository,
        IBookingRepository bookingRepository,
        IReferralValidator? referralValidator,
        int currentUserId)
    {
        this.screeningRepository = screeningRepository;
        this.eventRepository = eventRepository;
        this.userMovieDiscountRepository = userMovieDiscountRepository;
        this.priceWatcherRepository = priceWatcherRepository;
        this.userEventAttendanceRepository = userEventAttendanceRepository;
        this.bookingRepository = bookingRepository;
        this.referralValidator = referralValidator;
        this.currentUserId = currentUserId;
    }

    /// <summary>
    /// Raised when the user has finished the flow and wants to leave the page.
    /// </summary>
    public event Action? NavigateBack;

    /// <summary>
    /// Gets the screenings available for the current movie.
    /// </summary>
    public ObservableCollection<Screening> Screenings { get; } = new ObservableCollection<Screening>();

    /// <summary>
    /// Gets the movie being checked out.
    /// </summary>
    public Movie? Movie
    {
        get => this.movie;
        private set => this.SetProperty(ref this.movie, value);
    }

    /// <summary>
    /// Gets or sets the currently selected screening; setting it triggers an asynchronous load of the event details.
    /// </summary>
    public Screening? SelectedScreening
    {
        get => this.selectedScreening;
        set
        {
            if (this.SetProperty(ref this.selectedScreening, value))
            {
                _ = this.OnScreeningChangedAsync();
            }
        }
    }

    /// <summary>
    /// Gets the event details for the selected screening.
    /// </summary>
    public Event? CurrentEvent
    {
        get => this.currentEvent;
        private set
        {
            if (this.SetProperty(ref this.currentEvent, value))
            {
                this.OnPropertyChanged(nameof(this.SeatCapacity));
                this.OnPropertyChanged(nameof(this.HasEvent));
                this.OnPropertyChanged(nameof(this.EventLocation));
                this.OnPropertyChanged(nameof(this.EventTitle));
                this.OnPropertyChanged(nameof(this.EventDateTimeDisplay));
                this.OnPropertyChanged(nameof(this.UnitPriceDisplay));
            }
        }
    }

    /// <summary>
    /// Gets the seat capacity for the current event, or zero when no event is loaded.
    /// </summary>
    public int SeatCapacity => this.CurrentEvent?.MaxCapacity ?? 0;

    /// <summary>
    /// Gets a value indicating whether an event is currently loaded.
    /// </summary>
    public bool HasEvent => this.CurrentEvent is not null;

    /// <summary>
    /// Gets the location text for the current event.
    /// </summary>
    public string EventLocation => this.CurrentEvent?.LocationReference ?? string.Empty;

    /// <summary>
    /// Gets the title for the current event.
    /// </summary>
    public string EventTitle => this.CurrentEvent?.Title ?? string.Empty;

    /// <summary>
    /// Gets a formatted date/time string for the current event.
    /// </summary>
    public string EventDateTimeDisplay => this.CurrentEvent is null
        ? string.Empty
        : this.CurrentEvent.EventDateTime.ToString("F");

    /// <summary>
    /// Gets the formatted unit ticket price for the current event.
    /// </summary>
    public string UnitPriceDisplay => this.CurrentEvent is null
        ? string.Empty
        : this.CurrentEvent.TicketPrice.ToString("0.00");

    /// <summary>
    /// Gets or sets the number of seats the user has selected via the seat guide.
    /// </summary>
    public int SelectedSeatCount
    {
        get => this.selectedSeatCount;
        set
        {
            if (this.SetProperty(ref this.selectedSeatCount, value))
            {
                this.RecalculateTotals();
                this.OnPropertyChanged(nameof(this.CanPurchase));
            }
        }
    }

    /// <summary>
    /// Gets the base price (before discounts) for the current selection.
    /// </summary>
    public decimal BasePrice
    {
        get => this.basePrice;
        private set => this.SetProperty(ref this.basePrice, value);
    }

    /// <summary>
    /// Gets the final price after discounts, never below zero.
    /// </summary>
    public decimal FinalPrice
    {
        get => this.finalPrice;
        private set => this.SetProperty(ref this.finalPrice, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the free-enrollment reward should be applied.
    /// </summary>
    public bool ApplyFreeEnrollment
    {
        get => this.applyFreeEnrollment;
        set
        {
            if (this.SetProperty(ref this.applyFreeEnrollment, value))
            {
                this.RecalculateTotals();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the trivia free-ticket reward should be applied.
    /// </summary>
    public bool ApplyTriviaReward
    {
        get => this.applyTriviaReward;
        set
        {
            if (this.SetProperty(ref this.applyTriviaReward, value))
            {
                this.RecalculateTotals();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the slot-machine movie discount should be applied.
    /// </summary>
    public bool ApplySlotMachineDiscount
    {
        get => this.applySlotMachineDiscount;
        set
        {
            if (this.SetProperty(ref this.applySlotMachineDiscount, value))
            {
                this.RecalculateTotals();
            }
        }
    }

    /// <summary>
    /// Gets the percentage of the slot-machine discount available to the user (0 if none).
    /// </summary>
    public double SlotMachineDiscountPercent
    {
        get => this.slotMachineDiscountPercent;
        private set
        {
            if (this.SetProperty(ref this.slotMachineDiscountPercent, value))
            {
                this.OnPropertyChanged(nameof(this.HasSlotMachineDiscount));
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the user owns at least one slot-machine discount for this movie.
    /// </summary>
    public bool HasSlotMachineDiscount => this.SlotMachineDiscountPercent > 0;

    /// <summary>
    /// Gets or sets a value indicating whether the user is watching the current event for price changes.
    /// </summary>
    public bool IsWatchingPrice
    {
        get => this.isWatchingPrice;
        set => this.SetProperty(ref this.isWatchingPrice, value);
    }

    /// <summary>
    /// Gets the current status message shown to the user.
    /// </summary>
    public string StatusMessage
    {
        get => this.statusMessage;
        private set => this.SetProperty(ref this.statusMessage, value);
    }

    /// <summary>
    /// Gets a value indicating whether the user has already joined the current event.
    /// </summary>
    public bool HasUserJoined
    {
        get => this.hasUserJoined;
        private set
        {
            if (this.SetProperty(ref this.hasUserJoined, value))
            {
                this.OnPropertyChanged(nameof(this.CanPurchase));
            }
        }
    }

    /// <summary>
    /// Gets or sets the referral code text entered by the user.
    /// </summary>
    public string ReferralCode
    {
        get => this.referralCode;
        set => this.SetProperty(ref this.referralCode, value);
    }

    /// <summary>
    /// Gets the referral validation status text shown to the user.
    /// </summary>
    public string ReferralStatus
    {
        get => this.referralStatus;
        private set => this.SetProperty(ref this.referralStatus, value);
    }

    /// <summary>
    /// Gets a value indicating whether a load operation is currently in flight.
    /// </summary>
    public bool IsLoading
    {
        get => this.isLoading;
        private set => this.SetProperty(ref this.isLoading, value);
    }

    /// <summary>
    /// Gets a value indicating whether the user can submit a purchase right now.
    /// </summary>
    public bool CanPurchase => this.HasEvent
                                && !this.HasUserJoined
                                && this.SelectedSeatCount > 0
                                && !this.isPurchaseInFlight;

    /// <summary>
    /// Gets the seat-guide view model for the currently loaded event, regenerating it on event change.
    /// Loads real booked seats from the backend so they appear unavailable.
    /// </summary>
    public async Task<SeatGuideViewModel> GetOrCreateSeatGuideAsync()
    {
        if (this.seatGuide is not null)
        {
            return this.seatGuide;
        }

        if (this.CurrentEvent is null)
        {
            return new SeatGuideViewModel();
        }

        IReadOnlyList<(int Row, int Column)> booked = Array.Empty<(int, int)>();
        if (this.SelectedScreening is not null)
        {
            try
            {
                IReadOnlyList<SeatBooking> bookings =
                    await this.bookingRepository.GetByScreeningAsync(this.SelectedScreening.Id);
                booked = bookings.Select(b => (b.Row, b.Column)).ToList();
            }
            catch
            {
                booked = Array.Empty<(int, int)>();
            }
        }

        this.seatGuide = new SeatGuideViewModel(this.CurrentEvent.MaxCapacity, booked);
        this.seatGuide.RestoreSelection(this.confirmedSelection);
        return this.seatGuide;
    }

    /// <summary>
    /// Records the user's confirmed seat selection so it can be restored across dialog opens
    /// and submitted on purchase.
    /// </summary>
    public void ApplyConfirmedSelection(IReadOnlyList<(int Row, int Column)> selection)
    {
        this.confirmedSelection = selection ?? Array.Empty<(int, int)>();
        this.SelectedSeatCount = this.confirmedSelection.Count;
    }

    /// <summary>
    /// Loads screenings for the supplied movie and selects the requested screening (or the first one).
    /// </summary>
    /// <param name="movie">The movie being checked out.</param>
    /// <param name="initialScreening">An optional screening to pre-select.</param>
    /// <returns>A task representing the asynchronous load.</returns>
    public async Task LoadAsync(Movie movie, Screening? initialScreening)
    {
        this.Movie = movie ?? throw new ArgumentNullException(nameof(movie));
        this.IsLoading = true;
        this.StatusMessage = string.Empty;

        try
        {
            this.Screenings.Clear();
            IReadOnlyList<Screening> screenings = await this.screeningRepository.GetByMovieIdAsync(movie.Id);
            foreach (Screening screening in screenings.OrderBy(s => s.ScreeningTime))
            {
                this.Screenings.Add(screening);
            }

            Screening? target = initialScreening is not null
                ? this.Screenings.FirstOrDefault(s => s.Id == initialScreening.Id)
                : null;
            target ??= this.Screenings.FirstOrDefault();

            this.SelectedScreening = target;

            if (target is null)
            {
                this.StatusMessage = "No upcoming screenings are available for this movie.";
            }
        }
        catch (Exception ex)
        {
            this.StatusMessage = $"Failed to load screenings: {ex.Message}";
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    /// <summary>
    /// Forces a recompute of <see cref="BasePrice"/> and <see cref="FinalPrice"/>.
    /// </summary>
    public void RecalculateTotals()
    {
        if (this.CurrentEvent is null || this.SelectedSeatCount <= 0)
        {
            this.BasePrice = 0m;
            this.FinalPrice = 0m;
            return;
        }

        decimal unit = this.CurrentEvent.TicketPrice;
        decimal computedBase = unit * this.SelectedSeatCount;
        decimal price = computedBase;

        if (this.ApplyFreeEnrollment)
        {
            price = 0m;
        }
        else
        {
            if (this.ApplyTriviaReward)
            {
                price -= unit;
            }

            if (this.ApplySlotMachineDiscount && this.SlotMachineDiscountPercent > 0)
            {
                decimal multiplier = 1m - ((decimal)this.SlotMachineDiscountPercent / 100m);
                if (multiplier < 0m)
                {
                    multiplier = 0m;
                }

                price *= multiplier;
            }
        }

        if (price < 0m)
        {
            price = 0m;
        }

        this.BasePrice = computedBase;
        this.FinalPrice = decimal.Round(price, 2);
    }

    /// <summary>
    /// Validates the current referral code against the validator service.
    /// </summary>
    /// <returns>A task representing the asynchronous validation.</returns>
    public async Task ValidateReferralAsync()
    {
        if (string.IsNullOrWhiteSpace(this.ReferralCode))
        {
            this.ReferralStatus = string.Empty;
            return;
        }

        if (this.referralValidator is null)
        {
            this.ReferralStatus = "Referral validation is not available.";
            return;
        }

        try
        {
            bool isValid = await this.referralValidator.IsValidReferralAsync(this.ReferralCode, this.currentUserId);
            this.ReferralStatus = isValid ? "Referral code accepted." : "Referral code is not valid.";
        }
        catch (Exception ex)
        {
            this.ReferralStatus = $"Could not validate code: {ex.Message}";
        }
    }

    /// <summary>
    /// Toggles the price-watch flag for the current event.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ToggleWatchPriceAsync()
    {
        if (this.CurrentEvent is null)
        {
            return;
        }

        try
        {
            if (this.IsWatchingPrice)
            {
                await this.priceWatcherRepository.RemoveWatchAsync(this.CurrentEvent.Id);
                this.IsWatchingPrice = false;
                this.StatusMessage = "Stopped watching this event.";
            }
            else
            {
                bool added = await this.priceWatcherRepository.AddWatchAsync(new WatchedEvent
                {
                    EventId = this.CurrentEvent.Id,
                    EventTitle = this.CurrentEvent.Title,
                    TargetPrice = this.CurrentEvent.TicketPrice,
                });

                if (added)
                {
                    this.IsWatchingPrice = true;
                    this.StatusMessage = "Now watching this event for price changes.";
                }
                else
                {
                    this.StatusMessage = "Could not add this event to your watch list.";
                }
            }
        }
        catch (Exception ex)
        {
            this.StatusMessage = $"Watch toggle failed: {ex.Message}";
        }
    }

    /// <summary>
    /// Confirms the purchase, recording the user's attendance for the event and applying any redeemed reward.
    /// </summary>
    /// <returns>A task representing the asynchronous purchase.</returns>
    public async Task<bool> ConfirmPurchaseAsync()
    {
        if (!this.CanPurchase)
        {
            return false;
        }

        if (this.CurrentEvent is null)
        {
            this.StatusMessage = "Cannot complete: no event selected.";
            return false;
        }

        if (this.SelectedScreening is null)
        {
            this.StatusMessage = "Cannot complete: no screening selected.";
            return false;
        }

        if (this.confirmedSelection.Count == 0)
        {
            this.StatusMessage = "Please pick your seats in the visual guide before purchasing.";
            return false;
        }

        this.isPurchaseInFlight = true;
        this.OnPropertyChanged(nameof(this.CanPurchase));

        try
        {
            bool reserved = await this.bookingRepository.ReserveAsync(
                this.SelectedScreening.Id,
                this.currentUserId,
                this.confirmedSelection);

            if (!reserved)
            {
                this.StatusMessage = "One or more selected seats were just booked by someone else. Please pick again.";
                this.seatGuide = null;
                this.confirmedSelection = Array.Empty<(int, int)>();
                this.SelectedSeatCount = 0;
                return false;
            }

            await this.userEventAttendanceRepository.JoinAsync(this.currentUserId, this.CurrentEvent.Id);
            this.HasUserJoined = true;
            this.StatusMessage = $"Purchase confirmed for {this.SelectedSeatCount} seat(s). Final price: {this.FinalPrice:0.00}.";
            return true;
        }
        catch (Exception ex)
        {
            this.StatusMessage = $"Purchase failed: {ex.Message}";
            return false;
        }
        finally
        {
            this.isPurchaseInFlight = false;
            this.OnPropertyChanged(nameof(this.CanPurchase));
        }
    }

    /// <summary>
    /// Requests navigation back to the previous view.
    /// </summary>
    public void RequestBack() => this.NavigateBack?.Invoke();

    private async Task OnScreeningChangedAsync()
    {
        int generation = ++this.screeningChangeGeneration;

        this.seatGuide = null;
        this.confirmedSelection = Array.Empty<(int, int)>();
        this.SelectedSeatCount = 0;

        if (this.SelectedScreening is null)
        {
            this.CurrentEvent = null;
            this.HasUserJoined = false;
            this.SlotMachineDiscountPercent = 0;
            this.RecalculateTotals();
            return;
        }

        try
        {
            Event? eventDetails = await this.eventRepository.FindByIdAsync(this.SelectedScreening.EventId);

            if (generation != this.screeningChangeGeneration)
            {
                return;
            }

            this.CurrentEvent = eventDetails;

            if (eventDetails is null)
            {
                this.StatusMessage = "Event details could not be loaded.";
                this.HasUserJoined = false;
                this.SlotMachineDiscountPercent = 0;
            }
            else
            {
                await this.RefreshUserContextAsync(eventDetails);
                if (generation != this.screeningChangeGeneration)
                {
                    return;
                }
            }

            this.RecalculateTotals();
        }
        catch (Exception ex)
        {
            if (generation == this.screeningChangeGeneration)
            {
                this.StatusMessage = $"Failed to load event: {ex.Message}";
            }
        }
    }

    private async Task RefreshUserContextAsync(Event eventDetails)
    {
        try
        {
            IReadOnlyList<int> joined = await this.userEventAttendanceRepository.GetJoinedEventIdsAsync(this.currentUserId);
            this.HasUserJoined = joined.Contains(eventDetails.Id);
        }
        catch
        {
            this.HasUserJoined = false;
        }

        try
        {
            List<Reward> rewards = await this.userMovieDiscountRepository.GetDiscountsForUserAsync(this.currentUserId);
            string? movieTitle = this.Movie?.Title;
            int? movieId = this.Movie?.Id;
            Reward? best = rewards
                .Where(r => r.IsAvailable)
                .Where(r => string.Equals(r.RewardType, SlotMachineRewardType, StringComparison.OrdinalIgnoreCase))
                .Where(r =>
                    (movieId.HasValue && r.EventId == movieId.Value) ||
                    (movieTitle is not null && string.Equals(r.ApplicabilityScope, movieTitle, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(r => r.DiscountValue)
                .FirstOrDefault();
            double percent = best?.DiscountValue ?? 0;
            if (percent < 0)
            {
                percent = 0;
            }

            if (percent > 100)
            {
                percent = 100;
            }

            this.SlotMachineDiscountPercent = percent;
        }
        catch
        {
            this.SlotMachineDiscountPercent = 0;
        }

        try
        {
            List<WatchedEvent> watched = await this.priceWatcherRepository.GetAllWatchedEventsAsync();
            this.IsWatchingPrice = watched.Any(w => w.EventId == eventDetails.Id);
        }
        catch
        {
            this.IsWatchingPrice = false;
        }
    }
}
