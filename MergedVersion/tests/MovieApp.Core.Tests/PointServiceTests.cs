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

public class PointServiceAsyncTests
{
    private readonly Mock<IUserStatsRepository> statsRepoMock;
    private readonly Mock<IUserRepository> userRepoMock;
    private readonly Mock<IMovieRepository> movieRepoMock;
    private readonly Mock<IBadgeService> badgeServiceMock;
    private readonly PointService sut;

    public PointServiceAsyncTests()
    {
        statsRepoMock = new Mock<IUserStatsRepository>();
        userRepoMock = new Mock<IUserRepository>();
        movieRepoMock = new Mock<IMovieRepository>();
        badgeServiceMock = new Mock<IBadgeService>();

        sut = new PointService(
            statsRepoMock.Object,
            userRepoMock.Object,
            movieRepoMock.Object,
            badgeServiceMock.Object);
    }

    private User DefaultUser(int id = 1) => new() { Id = id, Username = $"user{id}", AuthProvider = "p", AuthSubject = "s" };

    private Movie DefaultMovie(int id = 10, double avgRating = 3.0) =>
        new() { Id = id, AverageRating = avgRating, Title = $"Movie {id}" };

    [Fact]
    public async Task GetUserStatsAsync_ReturnsExistingStats_WhenStatsAlreadyExist()
    {
        var user = DefaultUser();
        var existing = new UserStats { User = user, TotalPoints = 42, WeeklyScore = 10 };
        statsRepoMock.Setup(r => r.GetByUserIdAsync(user.Id, default)).ReturnsAsync(existing);

        var result = await sut.GetUserStatsAsync(user.Id);

        Assert.Equal(42, result.TotalPoints);
    }

    [Fact]
    public async Task GetUserStatsAsync_CreatesNewStats_WhenStatsDoNotExist()
    {
        var user = DefaultUser();
        statsRepoMock.Setup(r => r.GetByUserIdAsync(user.Id, default)).ReturnsAsync((UserStats?)null);
        userRepoMock.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);

        var result = await sut.GetUserStatsAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal(0, result.TotalPoints);
        statsRepoMock.Verify(r => r.InsertAsync(It.IsAny<UserStats>(), default), Times.Once);
    }

    [Fact]
    public async Task GetUserStatsAsync_ThrowsInvalidOperationException_WhenUserDoesNotExist()
    {
        statsRepoMock.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), default)).ReturnsAsync((UserStats?)null);
        userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), default)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.GetUserStatsAsync(999));
    }

    [Fact]
    public async Task AddPointsAsync_AddsTwoPoints_WhenMovieAverageRatingIsAbove3Point5()
    {
        var user = DefaultUser();
        var movie = DefaultMovie(avgRating: 4.0);
        var stats = new UserStats { User = user, TotalPoints = 0 };
        
        statsRepoMock.Setup(r => r.GetByUserIdAsync(user.Id, default)).ReturnsAsync(stats);
        movieRepoMock.Setup(r => r.GetByIdAsync(movie.Id, default)).ReturnsAsync(movie);

        await sut.AddPointsAsync(user.Id, movie.Id, isBattleMovie: false);

        Assert.Equal(2, stats.TotalPoints);
        statsRepoMock.Verify(r => r.UpdateAsync(stats, default), Times.Once);
        badgeServiceMock.Verify(s => s.CheckAndAwardBadgesAsync(user.Id, default), Times.Once);
    }

    [Fact]
    public async Task AddPointsAsync_AddsOnePoint_WhenMovieAverageRatingIsBelow2()
    {
        var user = DefaultUser();
        var movie = DefaultMovie(avgRating: 1.5);
        var stats = new UserStats { User = user, TotalPoints = 0 };

        statsRepoMock.Setup(r => r.GetByUserIdAsync(user.Id, default)).ReturnsAsync(stats);
        movieRepoMock.Setup(r => r.GetByIdAsync(movie.Id, default)).ReturnsAsync(movie);

        await sut.AddPointsAsync(user.Id, movie.Id, isBattleMovie: false);

        Assert.Equal(1, stats.TotalPoints);
    }

    [Fact]
    public async Task AddPointsAsync_AddsFiveExtraPoints_WhenIsBattleMovieIsTrue()
    {
        var user = DefaultUser();
        var movie = DefaultMovie(avgRating: 3.0);
        var stats = new UserStats { User = user, TotalPoints = 0 };

        statsRepoMock.Setup(r => r.GetByUserIdAsync(user.Id, default)).ReturnsAsync(stats);
        movieRepoMock.Setup(r => r.GetByIdAsync(movie.Id, default)).ReturnsAsync(movie);

        await sut.AddPointsAsync(user.Id, movie.Id, isBattleMovie: true);

        Assert.Equal(5, stats.TotalPoints);
    }

    [Fact]
    public async Task DeductPointsAsync_ReducesTotalPoints_BySpecifiedAmount()
    {
        var user = DefaultUser();
        var stats = new UserStats { User = user, TotalPoints = 20 };
        statsRepoMock.Setup(r => r.GetByUserIdAsync(user.Id, default)).ReturnsAsync(stats);

        await sut.DeductPointsAsync(user.Id, 8);

        Assert.Equal(12, stats.TotalPoints);
    }

    [Fact]
    public async Task FreezePointsAsync_DeductsAmount_WhenUserHasSufficientPoints()
    {
        var user = DefaultUser();
        var stats = new UserStats { User = user, TotalPoints = 50 };
        statsRepoMock.Setup(r => r.GetByUserIdAsync(user.Id, default)).ReturnsAsync(stats);

        await sut.FreezePointsAsync(user.Id, 20);

        Assert.Equal(30, stats.TotalPoints);
    }

    [Fact]
    public async Task FreezePointsAsync_ThrowsInvalidOperationException_WhenPointsAreInsufficient()
    {
        var user = DefaultUser();
        var stats = new UserStats { User = user, TotalPoints = 10 };
        statsRepoMock.Setup(r => r.GetByUserIdAsync(user.Id, default)).ReturnsAsync(stats);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.FreezePointsAsync(user.Id, 50));
    }

    [Fact]
    public async Task RefundPointsAsync_IncreasesTotalPoints_BySpecifiedAmount()
    {
        var user = DefaultUser();
        var stats = new UserStats { User = user, TotalPoints = 10 };
        statsRepoMock.Setup(r => r.GetByUserIdAsync(user.Id, default)).ReturnsAsync(stats);

        await sut.RefundPointsAsync(user.Id, 15);

        Assert.Equal(25, stats.TotalPoints);
    }

    [Fact]
    public async Task UpdateWeeklyScoreAsync_SetsWeeklyScore_ToCurrentTotalPoints()
    {
        var user = DefaultUser();
        var stats = new UserStats { User = user, TotalPoints = 77, WeeklyScore = 0 };
        statsRepoMock.Setup(r => r.GetByUserIdAsync(user.Id, default)).ReturnsAsync(stats);

        await sut.UpdateWeeklyScoreAsync(user.Id);

        Assert.Equal(77, stats.WeeklyScore);
    }
}