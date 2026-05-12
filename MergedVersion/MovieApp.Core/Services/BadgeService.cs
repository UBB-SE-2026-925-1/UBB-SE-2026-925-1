using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.DTOs;

#nullable enable

using MovieApp.Core.Interfaces.Repository;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.WebAPI.Controllers.DTOs;

namespace MovieApp.Core.Services;

/// <summary>
/// Provides business logic for badge management and achievement awarding.
/// </summary>
public sealed class BadgeService : IBadgeService
{
    private readonly IUserBadgeRepository userBadgeRepository;
    private readonly IBadgeRepository badgeRepository;
    private readonly IReviewRepository reviewRepository;
    private readonly IMovieRepository movieRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="BadgeService"/> class.
    /// </summary>
    /// <param name="userBadgeRepository">The user-badge link repository.</param>
    /// <param name="badgeRepository">The system badge definition repository.</param>
    /// <param name="reviewRepository">The movie review repository.</param>
    /// <param name="movieRepository">The movie metadata repository.</param>
    public BadgeService(
        IUserBadgeRepository userBadgeRepository,
        IBadgeRepository badgeRepository,
        IReviewRepository reviewRepository,
        IMovieRepository movieRepository)
    {
        this.userBadgeRepository = userBadgeRepository;
        this.badgeRepository = badgeRepository;
        this.reviewRepository = reviewRepository;
        this.movieRepository = movieRepository;
    }

    /// <inheritdoc/>
    public async Task<UserBadgesDTO> GetUserBadgesAsync(
    int userId,
    CancellationToken ct = default)
    {
        var badges = await this.badgeRepository
            .GetBadgesForUserAsync(userId, ct);

        return new UserBadgesDTO
        {
            UserId = userId,
            Badges = badges.Select(b => new BadgeDTO
            {
                BadgeId = b.BadgeId,
                Name = b.Name,
                Description = b.Description,
                CriteriaValue = b.CriteriaValue
            }).ToList()
        };
    }

    /// <inheritdoc/>
    public async Task<List<Badge>> GetAllBadgesAsync(CancellationToken ct = default)
    {
        return await this.badgeRepository.GetAllAsync(ct);
    }

    /// <inheritdoc/>
    public async Task CheckAndAwardBadgesAsync(int userId, CancellationToken ct = default)
    {
        var existingBadges = await this.userBadgeRepository.GetAllAsync(ct);
        var existingBadgeIds = existingBadges
            .Where(ub => ub.User?.Id == userId && ub.Badge is not null)
            .Select(ub => ub.Badge!.BadgeId)
            .ToList();

        var allBadges = await this.badgeRepository.GetAllAsync(ct);
        var allReviews = await this.reviewRepository.GetAllAsync(ct);

        var userReviews = allReviews.Where(r => r.User?.Id == userId).ToList();

        int totalReviews = userReviews.Count;
        int extraReviews = userReviews.Count(r => r.IsExtraReview);

        int fullyCompletedExtraReviews = userReviews.Count(r =>
            r.IsExtraReview &&
            !string.IsNullOrEmpty(r.CinematographyText) &&
            !string.IsNullOrEmpty(r.ActingText) &&
            !string.IsNullOrEmpty(r.CgiText) &&
            !string.IsNullOrEmpty(r.PlotText) &&
            !string.IsNullOrEmpty(r.SoundText));

        // Refactored for Genre collection
        int comedyReviews = userReviews.Count(r =>
            r.Movie?.Genres != null &&
            r.Movie.Genres.Any(g => g.Name.Equals("Comedy", StringComparison.OrdinalIgnoreCase)));

        double comedyPercentage = totalReviews > 0 ? (double)comedyReviews / totalReviews * 100 : 0;

        foreach (var badge in allBadges)
        {
            if (existingBadgeIds.Contains(badge.BadgeId))
            {
                continue;
            }

            bool shouldAward = badge.Name switch
            {
                "The Snob" => extraReviews >= 10,
                "Why so serious?" => fullyCompletedExtraReviews >= 50,
                "The Joker" => comedyPercentage > 70,
                "The Godfather I" => totalReviews >= 100,
                "The Godfather II" => totalReviews >= 200,
                "The Godfather III" => totalReviews >= 300,
                _ => false
            };

            if (shouldAward)
            {
                await this.userBadgeRepository.InsertAsync(new UserBadge
                {
                    // Minimal user object for persistence
                    User = new User { Id = userId, AuthProvider = string.Empty, AuthSubject = string.Empty, Username = string.Empty },
                    Badge = badge
                }, ct);
            }
        }
    }
}

