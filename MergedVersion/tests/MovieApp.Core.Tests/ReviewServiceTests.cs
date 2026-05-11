#nullable enable
using Moq;
using MovieApp.Core.Interfaces.Repository;
using MovieApp.Core.Repositories;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Core.Services;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MovieApp.Core.Tests;

public class ReviewServiceAsyncTests
{
    private readonly Mock<IReviewRepository> reviewRepoMock;
    private readonly Mock<IMovieRepository> movieRepoMock;
    private readonly Mock<IUserRepository> userRepoMock;
    private readonly Mock<IBattleRepository> battleRepoMock;
    private readonly Mock<IPointService> pointServiceMock;
    private readonly ReviewService sut;

    public ReviewServiceAsyncTests()
    {
        reviewRepoMock = new Mock<IReviewRepository>();
        movieRepoMock = new Mock<IMovieRepository>();
        userRepoMock = new Mock<IUserRepository>();
        battleRepoMock = new Mock<IBattleRepository>();
        pointServiceMock = new Mock<IPointService>();

        sut = new ReviewService(
            reviewRepoMock.Object,
            movieRepoMock.Object,
            userRepoMock.Object,
            battleRepoMock.Object,
            pointServiceMock.Object);
    }

    private User DefaultUser(int id = 1) => new() { Id = id, Username = $"user{id}", AuthProvider = "p", AuthSubject = "s" };
    private Movie DefaultMovie(int id = 10) => new() { Id = id, Title = "Test Movie", AverageRating = 3.0 };

    private string ValidContent(int length = 100) => new('x', length);

    [Fact]
    public async Task GetReviewsForMovieAsync_ReturnsOnlyReviewsForSpecifiedMovie()
    {
        var movie1 = DefaultMovie(10);
        var movie2 = DefaultMovie(20);
        var user = DefaultUser();
        var reviews = new List<Review>
        {
            new Review { ReviewId = 1, User = user, Movie = movie1, StarRating = 4f, Content = ValidContent() },
            new Review { ReviewId = 2, User = user, Movie = movie2, StarRating = 3f, Content = ValidContent() }
        };
        reviewRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(reviews);

        var result = await sut.GetReviewsForMovieAsync(10);

        Assert.Single(result);
        Assert.Equal(10, result[0].Movie!.Id);
    }

    [Fact]
    public async Task GetReviewsForMovieAsync_ReturnsEmptyList_WhenNoReviewsExist()
    {
        reviewRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Review>());

        var result = await sut.GetReviewsForMovieAsync(10);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetReviewsForMovieAsync_ExcludesReviews_WithStarRatingAboveFive()
    {
        var movie = DefaultMovie();
        var user = DefaultUser();
        var reviews = new List<Review>
        {
            new Review { ReviewId = 1, User = user, Movie = movie, StarRating = 6f, Content = ValidContent() }
        };
        reviewRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(reviews);

        var result = await sut.GetReviewsForMovieAsync(movie.Id);

        Assert.Empty(result);
    }

    [Fact]
    public async Task AddReviewAsync_ReturnsReview_WhenInputIsValid()
    {
        var user = DefaultUser();
        var movie = DefaultMovie();
        userRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(user);
        movieRepoMock.Setup(r => r.GetByIdAsync(10, default)).ReturnsAsync(movie);
        reviewRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Review>());
        battleRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Battle>());

        var result = await sut.AddReviewAsync(1, 10, 4.0f, ValidContent());

        Assert.NotNull(result);
        Assert.Equal(4.0f, result.StarRating);
        reviewRepoMock.Verify(r => r.InsertAsync(It.IsAny<Review>(), default), Times.Once);
    }

    [Fact]
    public async Task AddReviewAsync_ThrowsInvalidOperationException_WhenUserDoesNotExist()
    {
        userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), default)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.AddReviewAsync(999, 10, 4.0f, ValidContent()));
    }

    [Fact]
    public async Task AddReviewAsync_ThrowsInvalidOperationException_WhenUserAlreadyReviewedMovie()
    {
        var user = DefaultUser();
        var movie = DefaultMovie();
        var reviews = new List<Review>
        {
            new Review { User = user, Movie = movie }
        };
        userRepoMock.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        movieRepoMock.Setup(r => r.GetByIdAsync(movie.Id, default)).ReturnsAsync(movie);
        reviewRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(reviews);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.AddReviewAsync(user.Id, movie.Id, 4.0f, ValidContent()));
    }

    [Fact]
    public async Task AddReviewAsync_ThrowsInvalidOperationException_WhenRatingExceedsFive()
    {
        var user = DefaultUser();
        var movie = DefaultMovie();
        userRepoMock.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        movieRepoMock.Setup(r => r.GetByIdAsync(movie.Id, default)).ReturnsAsync(movie);
        reviewRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Review>());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.AddReviewAsync(user.Id, movie.Id, 5.5f, ValidContent()));
    }

    [Fact]
    public async Task GetAverageRatingAsync_ReturnsCorrectAverage_ForMultipleReviews()
    {
        var movie = DefaultMovie(10);
        var reviews = new List<Review>
        {
            new Review { Movie = movie, StarRating = 2f, Content = ValidContent() },
            new Review { Movie = movie, StarRating = 4f, Content = ValidContent() }
        };
        reviewRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(reviews);

        var result = await sut.GetAverageRatingAsync(10);

        Assert.Equal(3.0, result);
    }

    [Fact]
    public async Task UpdateReviewAsync_UpdatesRatingAndContent_WhenReviewExists()
    {
        var movie = DefaultMovie();
        var review = new Review { ReviewId = 1, Movie = movie, StarRating = 3f, Content = "Old" };
        reviewRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(review);
        movieRepoMock.Setup(r => r.GetByIdAsync(movie.Id, default)).ReturnsAsync(movie);
        reviewRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Review> { review });

        await sut.UpdateReviewAsync(1, 4.5f, "New content");

        Assert.Equal(4.5f, review.StarRating);
        Assert.Equal("New content", review.Content);
        reviewRepoMock.Verify(r => r.UpdateAsync(review, default), Times.Once);
    }

    [Fact]
    public async Task DeleteReviewAsync_RemovesReview_WhenReviewExists()
    {
        var movie = DefaultMovie();
        var review = new Review { ReviewId = 1, Movie = movie };
        reviewRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(review);
        movieRepoMock.Setup(r => r.GetByIdAsync(movie.Id, default)).ReturnsAsync(movie);
        reviewRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Review>());

        await sut.DeleteReviewAsync(1);

        reviewRepoMock.Verify(r => r.DeleteAsync(1, default), Times.Once);
    }

    [Fact]
    public async Task SubmitExtraReviewAsync_SetsIsExtraReviewToTrue_WhenInputIsValid()
    {
        var review = new Review { ReviewId = 1, IsExtraReview = false };
        reviewRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(review);

        await sut.SubmitExtraReviewAsync(1, 3, "cg", 3, "acting", 3, "plot", 3, "sound", 3, "cin", "main extra");

        Assert.True(review.IsExtraReview);
        Assert.Equal("main extra", review.Content);
        reviewRepoMock.Verify(r => r.UpdateAsync(review, default), Times.Once);
    }
}