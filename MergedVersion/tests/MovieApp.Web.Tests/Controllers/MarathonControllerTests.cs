using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using MovieApp.Core.DTOs;
using MovieApp.Core.Models;
using MovieApp.Core.Services;
using MovieApp.Web.Controllers;
using MovieApp.Web.Models;
using FluentAssertions;
using Xunit;

namespace MovieApp.Web.Tests.Controllers;

public class MarathonControllerTests
{
    private readonly Mock<IMarathonService> _mockMarathonService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly MarathonController _controller;

    public MarathonControllerTests()
    {
        _mockMarathonService = new Mock<IMarathonService>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();

        _mockCurrentUserService.Setup(s => s.CurrentUser).Returns(new CurrentUserDTO { Id = 1 });
        _mockCurrentUserService.Setup(s => s.InitializeAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _controller = new MarathonController(
            _mockMarathonService.Object,
            _mockCurrentUserService.Object)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithMarathons()
    {
        // Arrange
        var marathons = new List<Marathon> { new Marathon { Id = 1, Title = "Weekly Marathon" } };
        _mockMarathonService.Setup(s => s.GetWeeklyMarathonsAsync(1)).ReturnsAsync(marathons);
        _mockMarathonService.Setup(s => s.GetMarathonMovieCountAsync(1)).ReturnsAsync(5);
        _mockMarathonService.Setup(s => s.GetParticipantCountAsync(1)).ReturnsAsync(10);
        _mockMarathonService.Setup(s => s.GetUserProgressAsync(1, 1)).ReturnsAsync((MarathonProgress?)null);

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<MarathonIndexViewModel>().Subject;
        model.Items.Should().HaveCount(1);
        model.Items[0].Marathon.Title.Should().Be("Weekly Marathon");
    }

    [Fact]
    public async Task Details_ReturnsViewResult_WhenMarathonExists()
    {
        // Arrange
        var marathonId = 1;
        var marathons = new List<Marathon> { new Marathon { Id = marathonId, Title = "Marathon 1" } };
        _mockMarathonService.Setup(s => s.GetWeeklyMarathonsAsync(1)).ReturnsAsync(marathons);
        _mockMarathonService.Setup(s => s.GetMoviesForMarathonAsync(marathonId)).ReturnsAsync(new List<Movie>());
        _mockMarathonService.Setup(s => s.GetLeaderboardWithUsernamesAsync(marathonId)).ReturnsAsync(new List<LeaderboardEntry>());
        _mockMarathonService.Setup(s => s.GetUserProgressAsync(1, marathonId)).ReturnsAsync((MarathonProgress?)null);

        // Act
        var result = await _controller.Details(marathonId);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<MarathonDetailViewModel>().Subject;
        model.Marathon.Id.Should().Be(marathonId);
    }

    [Fact]
    public async Task Details_ReturnsNotFoundView_WhenMarathonDoesNotExist()
    {
        // Arrange
        _mockMarathonService.Setup(s => s.GetWeeklyMarathonsAsync(1)).ReturnsAsync(new List<Marathon>());

        // Act
        var result = await _controller.Details(999);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("NotFound");
    }

    [Fact]
    public async Task Start_RedirectsToDetails()
    {
        // Arrange
        var marathonId = 1;
        _mockMarathonService.Setup(s => s.StartMarathonAsync(marathonId)).ReturnsAsync(true);

        // Act
        var result = await _controller.Start(marathonId);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Details");
        redirectResult.RouteValues?["id"].Should().Be(marathonId);
        _controller.TempData["StatusMessage"].Should().Be("You joined the marathon.");
    }
}
