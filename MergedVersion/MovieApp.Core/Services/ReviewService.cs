using System.Threading;
using System.Threading.Tasks;
#nullable enable

using MovieApp.Core.Interfaces.Repository;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Core.Services;

/// <summary>
/// Provides logic for movie reviews, including validation and point awarding.
/// </summary>
public sealed class ReviewService : IReviewService
{
    private readonly IReviewRepository reviewRepository;
    private readonly IMovieRepository movieRepository;
    private readonly IUserRepository userRepository;
    private readonly IBattleRepository battleRepository;
    private readonly IPointService pointService;

    public ReviewService(
        IReviewRepository reviewRepository,
        IMovieRepository movieRepository,
        IUserRepository userRepository,
        IBattleRepository battleRepository,
        IPointService pointService)
    {
        this.reviewRepository = reviewRepository;
        this.movieRepository = movieRepository;
        this.userRepository = userRepository;
        this.battleRepository = battleRepository;
        this.pointService = pointService;
    }

    /// <inheritdoc/>
    public async Task<List<Review>> GetReviewsForMovieAsync(int movieId, CancellationToken ct = default)
    {
        var reviews = await this.reviewRepository.GetAllAsync(ct);
        return reviews
            .Where(r => r.Movie?.Id == movieId && r.StarRating <= 5)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<Review> AddReviewAsync(int userId, int movieId, float rating, string content, CancellationToken ct = default)
    {
        var user = await this.userRepository.GetByIdAsync(userId, ct) ?? throw new InvalidOperationException("User not found.");
        var movie = await this.movieRepository.GetByIdAsync(movieId, ct) ?? throw new InvalidOperationException("Movie not found.");

        var allReviews = await this.reviewRepository.GetAllAsync(ct);
        if (allReviews.Any(r => r.User?.Id == userId && r.Movie?.Id == movieId))
        {
            throw new InvalidOperationException("User has already reviewed this movie.");
        }

        if (rating < 0 || rating > 5) throw new InvalidOperationException("Rating must be 0-5.");
        if (content.Length > 2000) throw new InvalidOperationException("Review too long.");

        var review = new Review
        {
            User = user,
            Movie = movie,
            StarRating = rating,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            IsExtraReview = false
        };

        await this.reviewRepository.InsertAsync(review, ct);
        await this.RecalculateAverageRatingAsync(movieId, ct);

        var battles = await this.battleRepository.GetAllAsync(ct);
        bool isBattleMovie = battles.Any(b => b.Status == "Active" && (b.FirstMovie?.Id == movieId || b.SecondMovie?.Id == movieId));

        await this.pointService.AddPointsAsync(userId, movieId, isBattleMovie, ct);
        return review;
    }

    /// <inheritdoc/>
    public async Task UpdateReviewAsync(int reviewId, float rating, string content, CancellationToken ct = default)
    {
        var review = await this.reviewRepository.GetByIdAsync(reviewId, ct) ?? throw new InvalidOperationException("Review not found.");
        review.StarRating = rating;
        review.Content = content;

        await this.reviewRepository.UpdateAsync(review, ct);
        await this.RecalculateAverageRatingAsync(review.Movie?.Id ?? 0, ct);
    }

    /// <inheritdoc/>
    public async Task DeleteReviewAsync(int reviewId, CancellationToken ct = default)
    {
        var review = await this.reviewRepository.GetByIdAsync(reviewId, ct) ?? throw new InvalidOperationException("Review not found.");
        int movieId = review.Movie?.Id ?? 0;

        await this.reviewRepository.DeleteAsync(reviewId, ct);
        await this.RecalculateAverageRatingAsync(movieId, ct);
    }

    /// <inheritdoc/>
    public async Task SubmitExtraReviewAsync(int reviewId, int cgRating, string cgText, int actingRating, string actingText, int plotRating, string plotText, int soundRating, string soundText, int cinRating, string cinText, string mainExtraText, CancellationToken ct = default)
    {
        var review = await this.reviewRepository.GetByIdAsync(reviewId, ct) ?? throw new InvalidOperationException("Review not found.");

        review.IsExtraReview = true;
        review.Content = mainExtraText;
        review.CgiRating = cgRating;
        review.CgiText = cgText;
        review.ActingRating = actingRating;
        review.ActingText = actingText;
        review.PlotRating = plotRating;
        review.PlotText = plotText;
        review.SoundRating = soundRating;
        review.SoundText = soundText;
        review.CinematographyRating = cinRating;
        review.CinematographyText = cinText;

        await this.reviewRepository.UpdateAsync(review, ct);
    }

    /// <inheritdoc/>
    public async Task<double> GetAverageRatingAsync(int movieId, CancellationToken ct = default)
    {
        var reviews = await this.GetReviewsForMovieAsync(movieId, ct);
        return reviews.Count == 0 ? 0 : Math.Round(reviews.Average(r => r.StarRating), 1);
    }

    private async Task RecalculateAverageRatingAsync(int movieId, CancellationToken ct)
    {
        var movie = await this.movieRepository.GetByIdAsync(movieId, ct);
        if (movie != null)
        {
            movie.AverageRating = await this.GetAverageRatingAsync(movieId, ct);
            await this.movieRepository.UpdateAsync(movie, ct);
        }
    }
}

