// <copyright file="SlotMachineService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

/// <summary>
/// Orchestrates the slot machine logic, spins, and reward distribution.
/// </summary>
public sealed class SlotMachineService : ISlotMachineService
{
    private const int ResetSpinsCount = 5;
    private const int NoSpinsAvailable = 0;
    private const int MinDiscountPercentage = 10;
    private const int MaxDiscountPercentage = 70;
    private const int DiscountStep = 5;
    private const int RequiredLoginStreak = 3;
    private const int MaximumEventSpinsPerDay = 2;

    private readonly IUserSlotMachineStateRepository stateRepository;
    private readonly IMovieRepository movieRepository;
    private readonly IEventRepository eventRepository;
    private readonly IUserMovieDiscountRepository discountRepository;
    private readonly INotificationRepository notificationRepository;
    private readonly Random random = new Random();

    /// <summary>
    /// Initializes a new instance of the <see cref="SlotMachineService"/> class.
    /// </summary>
    /// <param name="stateRepository">The spin state repository.</param>
    /// <param name="movieRepository">The movie metadata repository.</param>
    /// <param name="eventRepository">The event repository.</param>
    /// <param name="discountRepository">The discount reward repository.</param>
    /// <param name="notificationRepository">The notification repository.</param>
    public SlotMachineService(
        IUserSlotMachineStateRepository stateRepository,
        IMovieRepository movieRepository,
        IEventRepository eventRepository,
        IUserMovieDiscountRepository discountRepository,
        INotificationRepository notificationRepository)
    {
        this.stateRepository = stateRepository;
        this.movieRepository = movieRepository;
        this.eventRepository = eventRepository;
        this.discountRepository = discountRepository;
        this.notificationRepository = notificationRepository;
    }

    /// <inheritdoc/>
    public async Task<SlotMachineResult> SpinAsync(int userIdentifier)
    {
        UserSpinData? state = await this.GetOrCreateUserStateAsync(userIdentifier);

        DateTime currentUtcDate = DateTime.UtcNow.Date;
        if (state.LastSlotSpinReset.Date < currentUtcDate)
        {
            state.ResetDailySpins(ResetSpinsCount);
        }

        int totalSpinsCount = state.DailySpinsRemaining + state.BonusSpins;
        if (totalSpinsCount <= NoSpinsAvailable)
        {
            throw new InvalidOperationException("No available spins");
        }

        if (state.DailySpinsRemaining > NoSpinsAvailable)
        {
            state.DailySpinsRemaining--;
        }
        else
        {
            state.BonusSpins--;
        }

        List<Genre> distinctGenres = (await this.movieRepository.GetGenresAsync()).ToList();
        List<Actor> distinctActors = (await this.movieRepository.GetActorsAsync()).ToList();
        List<Director> distinctDirectors = (await this.movieRepository.GetDirectorsAsync()).ToList();

        if (distinctGenres.Count == 0 || distinctActors.Count == 0 || distinctDirectors.Count == 0)
        {
            throw new InvalidOperationException("No genres, actors or directors available");
        }

        Genre selectedGenre = distinctGenres[this.random.Next(distinctGenres.Count)];
        Actor selectedActor = distinctActors[this.random.Next(distinctActors.Count)];
        Director selectedDirector = distinctDirectors[this.random.Next(distinctDirectors.Count)];

        IReadOnlyList<Event> matchingEvents = await this.GetMatchingEventsAsync(selectedGenre.Id, selectedActor.Id, selectedDirector.Id);
        Movie? jackpotMovie = await this.FindJackpotMovieAsync(selectedGenre.Id, selectedActor.Id, selectedDirector.Id);

        HashSet<int> jackpotEventIdentifiers = new HashSet<int>();
        if (jackpotMovie is not null)
        {
            IReadOnlyList<int> eventIdentifiers = await this.movieRepository.FindScreeningEventIdsForMovieAsync(jackpotMovie.Id);
            foreach (int eventIdentifier in eventIdentifiers)
            {
                jackpotEventIdentifiers.Add(eventIdentifier);
            }
        }

        SlotMachineResult result = new SlotMachineResult
        {
            Genre = selectedGenre,
            Actor = selectedActor,
            Director = selectedDirector,
            MatchingEvents = matchingEvents.ToList(),
            JackpotEventIds = jackpotEventIdentifiers,
            JackpotMovie = jackpotMovie,
            JackpotDiscountApplied = false,
            DiscountPercentage = 0,
        };

        if (jackpotMovie is not null)
        {
            int randomDiscount = this.RollRandomDiscount();
            await this.GrantJackpotDiscountInternal(userIdentifier, jackpotMovie.Id, randomDiscount);
            result.JackpotDiscountApplied = true;
            result.DiscountPercentage = randomDiscount;
        }

        await this.stateRepository.UpdateAsync(state);
        return result;
    }

    private int RollRandomDiscount()
    {
        int steps = (MaxDiscountPercentage - MinDiscountPercentage) / DiscountStep;
        return MinDiscountPercentage + (DiscountStep * this.random.Next(0, steps + 1));
    }

    /// <inheritdoc/>
    public async Task<int> GetAvailableSpinsAsync(int userIdentifier)
    {
        UserSpinData state = await this.GetOrCreateUserStateAsync(userIdentifier);

        DateTime currentUtcDate = DateTime.UtcNow.Date;
        if (state.LastSlotSpinReset.Date < currentUtcDate)
        {
            state.ResetDailySpins(ResetSpinsCount);
            await this.stateRepository.UpdateAsync(state);
        }

        return state.DailySpinsRemaining + state.BonusSpins;
    }

    /// <inheritdoc/>
    public async Task<UserSpinData> GetUserSpinStateAsync(int userIdentifier)
    {
        UserSpinData state = await this.GetOrCreateUserStateAsync(userIdentifier);

        DateTime currentUtcDate = DateTime.UtcNow.Date;
        if (state.LastSlotSpinReset.Date < currentUtcDate)
        {
            state.ResetDailySpins(ResetSpinsCount);
            await this.stateRepository.UpdateAsync(state);
        }

        return state;
    }

    /// <inheritdoc/>
    public async Task<bool> GrantBonusSpinForEventParticipationAsync(int userIdentifier)
    {
        UserSpinData state = await this.GetOrCreateUserStateAsync(userIdentifier);

        if (state.EventSpinRewardsToday < MaximumEventSpinsPerDay)
        {
            state.BonusSpins++;
            state.EventSpinRewardsToday++;
            await this.stateRepository.UpdateAsync(state);
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> RecordLoginAndCheckStreakAsync(int userIdentifier)
    {
        UserSpinData state = await this.GetOrCreateUserStateAsync(userIdentifier);

        state.UpdateLoginStreak();

        bool isBonusGranted = false;
        if (state.LoginStreak >= RequiredLoginStreak)
        {
            state.BonusSpins++;
            state.LoginStreak = 0;
            isBonusGranted = true;
        }

        await this.stateRepository.UpdateAsync(state);
        return isBonusGranted;
    }

    /// <inheritdoc/>
    public async Task<bool> GrantStreakSpinAsync(int userIdentifier)
    {
        UserSpinData state = await this.GetOrCreateUserStateAsync(userIdentifier);

        if (state.LoginStreak >= RequiredLoginStreak)
        {
            state.BonusSpins++;
            state.LoginStreak = 0;
            await this.stateRepository.UpdateAsync(state);
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public async Task<Genre> GetRandomGenreAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Genre> genres = await this.movieRepository.GetGenresAsync(cancellationToken);
        return genres[this.random.Next(genres.Count)];
    }

    /// <inheritdoc/>
    public async Task<Actor> GetRandomActorAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Actor> actors = await this.movieRepository.GetActorsAsync(cancellationToken);
        return actors[this.random.Next(actors.Count)];
    }

    /// <inheritdoc/>
    public async Task<Director> GetRandomDirectorAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Director> directors = await this.movieRepository.GetDirectorsAsync(cancellationToken);
        return directors[this.random.Next(directors.Count)];
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Genre>> GetGenresAsync(CancellationToken cancellationToken = default)
    {
        return await this.movieRepository.GetGenresAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Actor>> GetActorsAsync(CancellationToken cancellationToken = default)
    {
        return await this.movieRepository.GetActorsAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Director>> GetDirectorsAsync(CancellationToken cancellationToken = default)
    {
        return await this.movieRepository.GetDirectorsAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Event>> GetMatchingEventsAsync(int genreIdentifier, int actorIdentifier, int directorIdentifier)
    {
        IReadOnlyList<Movie> matchingMovies = await this.movieRepository.FindMoviesByAnyCriteriaAsync(genreIdentifier, actorIdentifier, directorIdentifier);
        List<Event> resultEvents = new List<Event>();

        foreach (Movie movie in matchingMovies)
        {
            IReadOnlyList<int> eventIdentifiers = await this.movieRepository.FindScreeningEventIdsForMovieAsync(movie.Id);
            IEnumerable<Event> allEvents = await this.eventRepository.GetAllAsync();
            resultEvents.AddRange(allEvents.Where(eventEntity => eventIdentifiers.Contains(eventEntity.Id)
                && eventEntity.EventDateTime > DateTime.UtcNow));
        }

        return resultEvents.DistinctBy(eventEntity => eventEntity.Id).ToList();
    }

    /// <inheritdoc/>
    public async Task<Movie?> FindJackpotMovieAsync(int genreIdentifier, int actorIdentifier, int directorIdentifier)
    {
        IReadOnlyList<Movie> movies = await this.movieRepository.FindMoviesByCriteriaAsync(genreIdentifier, actorIdentifier, directorIdentifier);
        return movies.FirstOrDefault();
    }

    /// <inheritdoc/>
    public Task GrantJackpotDiscount(int userIdentifier, int movieIdentifier)
        => this.GrantJackpotDiscountInternal(userIdentifier, movieIdentifier, this.RollRandomDiscount());

    private async Task GrantJackpotDiscountInternal(int userIdentifier, int movieIdentifier, int discountPercentage)
    {
        var movie = await this.movieRepository.GetByIdAsync(movieIdentifier);
        string movieTitle = movie?.Title ?? "a movie";

        Reward jackpotReward = new Reward
        {
            RewardId = 0,
            RewardType = "MovieDiscount",
            RedemptionStatus = false,
            ApplicabilityScope = movieTitle,
            DiscountValue = discountPercentage,
            OwnerUserId = userIdentifier,
            EventId = movieIdentifier,
        };

        await this.discountRepository.AddAsync(jackpotReward);

        Notification notification = new Notification
        {
            Id = 0,
            UserId = userIdentifier,
            EventId = 0,
            Type = "Jackpot Win",
            Message = $"Congratulations! You won a {discountPercentage}% discount for the movie '{movieTitle}' through the Slot Machine!",
            CreatedAt = DateTime.UtcNow,
            State = NotificationState.Unread
        };

        await this.notificationRepository.AddAsync(notification);
    }

    private async Task<UserSpinData> GetOrCreateUserStateAsync(int userIdentifier)
    {
        UserSpinData? state = await this.stateRepository.GetByUserIdAsync(userIdentifier);
        if (state == null)
        {
            state = new UserSpinData
            {
                UserId = userIdentifier,
                DailySpinsRemaining = ResetSpinsCount,
                LastSlotSpinReset = DateTime.UtcNow.Date,
                BonusSpins = 0,
                EventSpinRewardsToday = 0,
                LoginStreak = 0,
                LastLoginDate = DateTime.MinValue
            };
            await this.stateRepository.CreateAsync(state);
        }
        return state;
    }
}
