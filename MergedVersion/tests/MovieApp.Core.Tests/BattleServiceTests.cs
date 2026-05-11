#nullable enable
using Moq;
using MovieApp.Core.Interfaces;
using MovieApp.Core.Repositories;
using MovieApp.Core.Interfaces.Repository;
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

public class BattleServiceTests
{
    private readonly Mock<IBattleRepository> battleRepoMock;
    private readonly Mock<IBetRepository> betRepoMock;
    private readonly Mock<IMovieRepository> movieRepoMock;
    private readonly Mock<IUserRepository> userRepoMock;
    private readonly Mock<IPointService> pointServiceMock;
    private readonly BattleService sut;

    public BattleServiceTests()
    {
        battleRepoMock = new Mock<IBattleRepository>();
        betRepoMock = new Mock<IBetRepository>();
        movieRepoMock = new Mock<IMovieRepository>();
        userRepoMock = new Mock<IUserRepository>();
        pointServiceMock = new Mock<IPointService>();

        sut = new BattleService(
            battleRepoMock.Object,
            betRepoMock.Object,
            movieRepoMock.Object,
            userRepoMock.Object,
            pointServiceMock.Object);
    }

    // --- GetActiveBattleAsync ---
    [Fact]
    public async Task GetActiveBattleAsync_WhenActiveBattleExists_ReturnsBattleWithMoviesLoaded()
    {
        var movie1 = new Movie { Id = 1, Title = "Inception" };
        var movie2 = new Movie { Id = 2, Title = "Avatar" };
        var battle = new Battle
        {
            BattleId = 1,
            Status = "Active",
            FirstMovie = new Movie { Id = 1 },
            SecondMovie = new Movie { Id = 2 }
        };
        battleRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Battle> { battle });
        movieRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(movie1);
        movieRepoMock.Setup(r => r.GetByIdAsync(2, default)).ReturnsAsync(movie2);

        var result = await sut.GetActiveBattleAsync();

        Assert.NotNull(result);
        Assert.Equal("Active", result.Status);
        Assert.Equal("Inception", result.FirstMovie!.Title);
    }

    [Fact]
    public async Task GetActiveBattleAsync_WhenNoActiveBattle_ReturnsNull()
    {
        battleRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Battle>
        {
            new () { BattleId = 1, Status = "Finished" }
        });

        var result = await sut.GetActiveBattleAsync();

        Assert.Null(result);
    }

    // --- CreateBattleAsync ---
    [Fact]
    public async Task CreateBattleAsync_CreatesBattle_WhenNoActiveBattleExists()
    {
        var movie1 = new Movie { Id = 1, AverageRating = 4.0 };
        var movie2 = new Movie { Id = 2, AverageRating = 4.3 };
        battleRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Battle>());
        movieRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(movie1);
        movieRepoMock.Setup(r => r.GetByIdAsync(2, default)).ReturnsAsync(movie2);

        var result = await sut.CreateBattleAsync(1, 2);

        Assert.Equal("Active", result.Status);
        Assert.Equal(1, result.FirstMovie!.Id);
        battleRepoMock.Verify(r => r.InsertAsync(It.IsAny<Battle>(), default), Times.Once);
    }

    [Fact]
    public async Task CreateBattleAsync_WhenActiveBattleAlreadyExists_ThrowsInvalidOperationException()
    {
        battleRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Battle>
        {
            new () { BattleId = 1, Status = "Active" }
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.CreateBattleAsync(1, 2));
    }

    [Fact]
    public async Task CreateBattleAsync_WhenFirstMovieNotFound_ThrowsInvalidOperationException()
    {
        battleRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Battle>());
        movieRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync((Movie?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.CreateBattleAsync(1, 2));
    }

    // --- PlaceBetAsync ---
    [Fact]
    public async Task PlaceBetAsync_WhenValidBet_CreatesBetAndFreezesPoints()
    {
        var user = new User { Id = 1, AuthProvider = "test", AuthSubject = "test", Username = "user1" };
        var battle = new Battle { BattleId = 1, Status = "Active" };
        var movie = new Movie { Id = 2 };

        betRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Bet>());
        userRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(user);
        battleRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(battle);
        movieRepoMock.Setup(r => r.GetByIdAsync(2, default)).ReturnsAsync(movie);
        pointServiceMock.Setup(p => p.FreezePointsAsync(1, 50, default)).Returns(Task.CompletedTask);

        var result = await sut.PlaceBetAsync(1, 1, 2, 50);

        Assert.Equal(50, result.Amount);
        pointServiceMock.Verify(p => p.FreezePointsAsync(1, 50, default), Times.Once);
        betRepoMock.Verify(r => r.InsertAsync(It.IsAny<Bet>(), default), Times.Once);
    }

    [Fact]
    public async Task PlaceBetAsync_WhenAmountIsZero_ThrowsInvalidOperationException()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.PlaceBetAsync(1, 1, 2, 0));
    }

    [Fact]
    public async Task PlaceBetAsync_WhenAmountIsNegative_ThrowsInvalidOperationException()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.PlaceBetAsync(1, 1, 2, -10));
    }

    [Fact]
    public async Task PlaceBetAsync_WhenUserAlreadyBetOnBattle_ThrowsInvalidOperationException()
    {
        var existingBet = new Bet
        {
            User = new User { Id = 1, AuthProvider = "test", AuthSubject = "test", Username = "user1" },
            Battle = new Battle { BattleId = 1 },
            Movie = new Movie { Id = 2 },
            Amount = 20
        };
        betRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Bet> { existingBet });

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.PlaceBetAsync(1, 1, 2, 10));
    }

    // --- GetBetAsync ---
    [Fact]
    public async Task GetBetAsync_WhenBetExists_ReturnsBet()
    {
        var bet = new Bet
        {
            User = new User { Id = 1, AuthProvider = "test", AuthSubject = "test", Username = "user1" },
            Battle = new Battle { BattleId = 1 },
            Movie = new Movie { Id = 2 },
            Amount = 30
        };
        betRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Bet> { bet });

        var result = await sut.GetBetAsync(1, 1);

        Assert.NotNull(result);
        Assert.Equal(30, result.Amount);
    }

    [Fact]
    public async Task GetBetAsync_WhenBetDoesNotExist_ReturnsNull()
    {
        betRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Bet>());

        var result = await sut.GetBetAsync(1, 1);

        Assert.Null(result);
    }

    // --- DetermineWinnerAsync ---
    [Fact]
    public async Task DetermineWinnerAsync_WhenFirstMovieImprovedMore_ReturnsFirstMovieId()
    {
        var battle = new Battle
        {
            BattleId = 1,
            InitialRatingFirstMovie = 3.0,
            InitialRatingSecondMovie = 3.0,
            FirstMovie = new Movie { Id = 10 },
            SecondMovie = new Movie { Id = 20 }
        };
        var movie1 = new Movie { Id = 10, AverageRating = 4.5 }; // improved by 1.5
        var movie2 = new Movie { Id = 20, AverageRating = 3.5 }; // improved by 0.5

        battleRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(battle);
        movieRepoMock.Setup(r => r.GetByIdAsync(10, default)).ReturnsAsync(movie1);
        movieRepoMock.Setup(r => r.GetByIdAsync(20, default)).ReturnsAsync(movie2);

        var winner = await sut.DetermineWinnerAsync(1);

        Assert.Equal(10, winner);
    }

    [Fact]
    public async Task DetermineWinnerAsync_WhenSecondMovieImprovedMore_ReturnsSecondMovieId()
    {
        var battle = new Battle
        {
            BattleId = 1,
            InitialRatingFirstMovie = 3.0,
            InitialRatingSecondMovie = 3.0,
            FirstMovie = new Movie { Id = 10 },
            SecondMovie = new Movie { Id = 20 }
        };
        var movie1 = new Movie { Id = 10, AverageRating = 3.2 }; // improved by 0.2
        var movie2 = new Movie { Id = 20, AverageRating = 4.5 }; // improved by 1.5

        battleRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(battle);
        movieRepoMock.Setup(r => r.GetByIdAsync(10, default)).ReturnsAsync(movie1);
        movieRepoMock.Setup(r => r.GetByIdAsync(20, default)).ReturnsAsync(movie2);

        var winner = await sut.DetermineWinnerAsync(1);

        Assert.Equal(20, winner);
    }

    [Fact]
    public async Task DetermineWinnerAsync_WhenBattleNotFound_ThrowsInvalidOperationException()
    {
        battleRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), default)).ReturnsAsync((Battle?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.DetermineWinnerAsync(999));
    }

    // --- DistributePayoutsAsync ---
    [Fact]
    public async Task DistributePayoutsAsync_WhenWinnersExist_RefundsDoubleAmountToWinners()
    {
        var battle = new Battle
        {
            BattleId = 1,
            InitialRatingFirstMovie = 3.0,
            InitialRatingSecondMovie = 3.0,
            FirstMovie = new Movie { Id = 10 },
            SecondMovie = new Movie { Id = 20 },
            Status = "Active"
        };
        var winnerMovie = new Movie { Id = 10, AverageRating = 4.5 };
        var loserMovie = new Movie { Id = 20, AverageRating = 3.2 };

        var winnerBet = new Bet { User = new User { Id = 1, AuthProvider = "test", AuthSubject = "test", Username = "user1" }, Battle = new Battle { BattleId = 1 }, Movie = new Movie { Id = 10 }, Amount = 50 };
        var loserBet = new Bet { User = new User { Id = 2, AuthProvider = "test", AuthSubject = "test", Username = "user2" }, Battle = new Battle { BattleId = 1 }, Movie = new Movie { Id = 20 }, Amount = 30 };

        battleRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(battle);
        movieRepoMock.Setup(r => r.GetByIdAsync(10, default)).ReturnsAsync(winnerMovie);
        movieRepoMock.Setup(r => r.GetByIdAsync(20, default)).ReturnsAsync(loserMovie);
        betRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Bet> { winnerBet, loserBet });
        pointServiceMock.Setup(p => p.RefundPointsAsync(It.IsAny<int>(), It.IsAny<int>(), default)).Returns(Task.CompletedTask);

        await sut.DistributePayoutsAsync(1);

        pointServiceMock.Verify(p => p.RefundPointsAsync(1, 100, default), Times.Once); // winner gets 50*2
        pointServiceMock.Verify(p => p.RefundPointsAsync(2, It.IsAny<int>(), default), Times.Never); // loser gets nothing
        battleRepoMock.Verify(r => r.UpdateAsync(It.Is<Battle>(b => b.Status == "Finished"), default), Times.Once);
    }

    // --- GetCurrentBattleForUserAsync ---
    [Fact]
    public async Task GetCurrentBattleForUserAsync_WhenActiveBattleExists_ReturnsActiveBattle()
    {
        var movie1 = new Movie { Id = 1 };
        var movie2 = new Movie { Id = 2 };
        var activeBattle = new Battle { BattleId = 1, Status = "Active", FirstMovie = movie1, SecondMovie = movie2 };
        battleRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Battle> { activeBattle });
        movieRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(movie1);
        movieRepoMock.Setup(r => r.GetByIdAsync(2, default)).ReturnsAsync(movie2);

        var result = await sut.GetCurrentBattleForUserAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Active", result.Status);
    }

    // --- SettleExpiredBattlesAsync ---
    [Fact]
    public async Task SettleExpiredBattlesAsync_WhenActiveBattleIsExpired_DistributesPayoutsAndMarksFinished()
    {
        var expiredBattle = new Battle
        {
            BattleId = 1,
            Status = "Active",
            EndDate = DateTime.UtcNow.AddDays(-1),
            InitialRatingFirstMovie = 3.0,
            InitialRatingSecondMovie = 3.0,
            FirstMovie = new Movie { Id = 10 },
            SecondMovie = new Movie { Id = 20 }
        };
        var movie1 = new Movie { Id = 10, AverageRating = 4.0 };
        var movie2 = new Movie { Id = 20, AverageRating = 3.5 };

        battleRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Battle> { expiredBattle });
        battleRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(expiredBattle);
        movieRepoMock.Setup(r => r.GetByIdAsync(10, default)).ReturnsAsync(movie1);
        movieRepoMock.Setup(r => r.GetByIdAsync(20, default)).ReturnsAsync(movie2);
        betRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Bet>());
        pointServiceMock.Setup(p => p.RefundPointsAsync(It.IsAny<int>(), It.IsAny<int>(), default)).Returns(Task.CompletedTask);

        await sut.SettleExpiredBattlesAsync();

        battleRepoMock.Verify(r => r.UpdateAsync(It.Is<Battle>(b => b.Status == "Finished"), default), Times.Once);
    }
}
