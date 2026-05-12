using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Services;
using MovieApp.Core.DTOs;
using MovieApp.Core.Models;
using MovieApp.Web.Controllers;
using MovieApp.Web.Models;

namespace MovieApp.Web.Tests.Controllers;

public class BattleControllerTests
{
    private readonly Mock<IBattleService> _mockBattleService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IPointService> _mockPointService;
    private readonly BattleController _controller;

    public BattleControllerTests()
    {
        _mockBattleService = new Mock<IBattleService>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockPointService = new Mock<IPointService>();

        _mockCurrentUserService.Setup(s => s.CurrentUser).Returns(new CurrentUserDTO { Id = 1 });
        _mockCurrentUserService.Setup(s => s.InitializeAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _controller = new BattleController(
            _mockBattleService.Object,
            _mockCurrentUserService.Object,
            _mockPointService.Object)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithViewModel()
    {
        // Arrange
        var userId = 1;
        var userStats = new UserStats { TotalPoints = 100 };
        var battle = new Battle { BattleId = 1, Status = "Active" };
        
        _mockPointService.Setup(s => s.GetUserStatsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(userStats);
        _mockBattleService.Setup(s => s.GetCurrentBattleForUserAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(battle);
        _mockBattleService.Setup(s => s.GetBetAsync(userId, 1, It.IsAny<CancellationToken>())).ReturnsAsync((Bet)null);

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<BattleViewModel>().Subject;
        model.Battle.Should().Be(battle);
        model.CurrentUserPoints.Should().Be(100);
    }

    [Fact]
    public async Task PlaceBet_RedirectsToIndex_OnSuccess()
    {
        // Arrange
        var model = new PlaceBattleBetInputModel { BattleId = 1, MovieId = 10, Amount = 50 };

        // Act
        var result = await _controller.PlaceBet(model);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
        _mockBattleService.Verify(s => s.PlaceBetAsync(1, 1, 10, 50, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PlaceBet_RedirectsToIndex_OnFailure()
    {
        // Arrange
        var model = new PlaceBattleBetInputModel { BattleId = 1, MovieId = 10, Amount = 5000 };
        _mockBattleService.Setup(s => s.PlaceBetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Not enough points"));

        // Act
        var result = await _controller.PlaceBet(model);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
        _controller.TempData["StatusMessage"].ToString().Should().Contain("Could not place bet");
    }
}
