using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MovieApp.Core.Models;
using MovieApp.Core.Services;
using MovieApp.Core.DTOs;
using MovieApp.Web.Controllers;
using MovieApp.Web.Models;

namespace MovieApp.Web.Tests.Controllers;

public class SlotMachineControllerTests
{
    private readonly Mock<ISlotMachineService> _mockSlotMachineService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly SlotMachineController _controller;

    public SlotMachineControllerTests()
    {
        _mockSlotMachineService = new Mock<ISlotMachineService>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        
        // Mock current user
        _mockCurrentUserService.Setup(s => s.CurrentUser).Returns(new CurrentUserDTO { Id = 1, Username = "testuser" });
        _mockCurrentUserService.Setup(s => s.InitializeAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _controller = new SlotMachineController(_mockSlotMachineService.Object, _mockCurrentUserService.Object)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithViewModel()
    {
        // Arrange
        var userId = 1;
        var spinState = new UserSpinData { UserId = userId, DailySpinsRemaining = 3, BonusSpins = 0, LoginStreak = 1 };
        _mockSlotMachineService.Setup(s => s.GetUserSpinStateAsync(userId)).ReturnsAsync(spinState);
        _mockSlotMachineService.Setup(s => s.GetAvailableSpinsAsync(userId)).ReturnsAsync(3);

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<SlotMachineIndexViewModel>().Subject;
        model.AvailableSpins.Should().Be(3);
        model.DailySpinsRemaining.Should().Be(3);
    }

    [Fact]
    public async Task Spin_WhenCanSpin_CallsSpinAsync_AndReturnsView()
    {
        // Arrange
        var userId = 1;
        var spinState = new UserSpinData { UserId = userId, DailySpinsRemaining = 3, BonusSpins = 0, LoginStreak = 1 };
        var spinResult = new SlotMachineResult { JackpotDiscountApplied = false, MatchingEvents = new List<Event>() };
        
        _mockSlotMachineService.Setup(s => s.GetUserSpinStateAsync(userId)).ReturnsAsync(spinState);
        _mockSlotMachineService.Setup(s => s.GetAvailableSpinsAsync(userId)).ReturnsAsync(3);
        _mockSlotMachineService.Setup(s => s.SpinAsync(userId)).ReturnsAsync(spinResult);

        // Act
        var result = await _controller.Spin();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("Index");
        _mockSlotMachineService.Verify(s => s.SpinAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Spin_WhenCannotSpin_ReturnsViewWithErrorMessage()
    {
        // Arrange
        var userId = 1;
        var spinState = new UserSpinData { UserId = userId, DailySpinsRemaining = 0, BonusSpins = 0, LoginStreak = 1 };
        
        _mockSlotMachineService.Setup(s => s.GetUserSpinStateAsync(userId)).ReturnsAsync(spinState);
        _mockSlotMachineService.Setup(s => s.GetAvailableSpinsAsync(userId)).ReturnsAsync(0);

        // Act
        var result = await _controller.Spin();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("Index");
        var model = viewResult.Model.Should().BeOfType<SlotMachineIndexViewModel>().Subject;
        model.StatusMessage.Should().Be("No spins remaining - come back tomorrow!");
        _mockSlotMachineService.Verify(s => s.SpinAsync(It.IsAny<int>()), Times.Never);
    }
}
