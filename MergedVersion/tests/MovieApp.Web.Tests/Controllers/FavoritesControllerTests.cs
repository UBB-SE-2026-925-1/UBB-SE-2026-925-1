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

public class FavoritesControllerTests
{
    private readonly Mock<IFavoriteEventService> _mockFavoriteService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly FavoritesController _controller;

    public FavoritesControllerTests()
    {
        _mockFavoriteService = new Mock<IFavoriteEventService>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();

        _mockCurrentUserService.Setup(s => s.CurrentUser).Returns(new CurrentUserDTO { Id = 1 });
        _mockCurrentUserService.Setup(s => s.InitializeAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _controller = new FavoritesController(
            _mockFavoriteService.Object,
            _mockCurrentUserService.Object)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithFavoriteEvents()
    {
        // Arrange
        var events = new List<Event> { new Event { Id = 1, Title = "Event 1", EventDateTime = DateTime.Now, LocationReference = "Ref", TicketPrice = 10, CreatorUserId = 1 } };
        _mockFavoriteService.Setup(s => s.GetFavoriteEventsByUserIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(events);

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<FavoriteIndexViewModel>().Subject;
        model.Events.Should().BeEquivalentTo(events);
    }

    [Fact]
    public async Task Add_RedirectsToIndex_WhenSuccessful()
    {
        // Arrange
        var input = new FavoriteActionInputModel { EventId = 1, ReturnUrl = null };
        _mockFavoriteService.Setup(s => s.ExistsFavoriteAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _controller.Add(input);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
        _mockFavoriteService.Verify(s => s.AddFavoriteAsync(1, 1, It.IsAny<CancellationToken>()), Times.Once);
        _controller.TempData["StatusMessage"].Should().Be("Added to favorites.");
    }

    [Fact]
    public async Task Remove_RedirectsToIndex_WhenSuccessful()
    {
        // Arrange
        var input = new FavoriteActionInputModel { EventId = 1, ReturnUrl = null };
        _mockFavoriteService.Setup(s => s.ExistsFavoriteAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _controller.Remove(input);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
        _mockFavoriteService.Verify(s => s.RemoveFavoriteAsync(1, 1, It.IsAny<CancellationToken>()), Times.Once);
        _controller.TempData["StatusMessage"].Should().Be("Removed from favorites.");
    }
}
