using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Repositories;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Services;
using MovieApp.Core.DTOs;
using MovieApp.Core.Models;
using MovieApp.Proxy;
using MovieApp.Web.Controllers;
using MovieApp.Web.Models;
using System.Net;
using System.Threading;

namespace MovieApp.Web.Tests.Controllers;

public class ScreeningControllerTests
{
    private readonly Mock<IScreeningRepository> _mockScreeningRepo;
    private readonly Mock<IBookingRepository> _mockBookingRepo;
    private readonly Mock<ICatalogService> _mockCatalogService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly ScreeningController _controller;

    public ScreeningControllerTests()
    {
        _mockScreeningRepo = new Mock<IScreeningRepository>();
        _mockBookingRepo = new Mock<IBookingRepository>();
        _mockCatalogService = new Mock<ICatalogService>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();

        var handlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/") };
        var apiClient = new ApiClient(httpClient);

        _mockCurrentUserService.Setup(s => s.CurrentUser).Returns(new CurrentUserDTO { Id = 1 });
        _mockCurrentUserService.Setup(s => s.InitializeAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _controller = new ScreeningController(
            _mockScreeningRepo.Object,
            _mockBookingRepo.Object,
            _mockCatalogService.Object,
            _mockCurrentUserService.Object,
            apiClient);
    }

    [Fact]
    public async Task ForMovie_ReturnsViewResult_WhenMovieExists()
    {
        // Arrange
        var movieId = 1;
        var movie = new Movie { Id = movieId, Title = "Movie 1" };
        _mockCatalogService.Setup(s => s.GetMovieByIdAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(movie);
        _mockScreeningRepo.Setup(r => r.GetByMovieIdAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Screening>());

        // Act
        var result = await _controller.ForMovie(movieId);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<ScreeningListViewModel>().Subject;
        model.Movie.Should().Be(movie);
    }

    [Fact]
    public async Task ForMovie_ReturnsNotFound_WhenMovieDoesNotExist()
    {
        // Arrange
        var movieId = 999;
        _mockCatalogService.Setup(s => s.GetMovieByIdAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync((Movie)null);

        // Act
        var result = await _controller.ForMovie(movieId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
