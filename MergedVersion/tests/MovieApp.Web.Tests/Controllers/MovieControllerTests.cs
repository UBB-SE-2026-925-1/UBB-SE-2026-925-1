using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MovieApp.Core.Interfaces;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Core.Services;
using MovieApp.Core.DTOs;
using MovieApp.Web.Controllers;
using MovieApp.Web.Models;
using System.Threading;

namespace MovieApp.Web.Tests.Controllers;

public class MovieControllerTests
{
    private readonly Mock<ICatalogService> _mockCatalogService;
    private readonly Mock<IReviewService> _mockReviewService;
    private readonly Mock<ICommentService> _mockCommentService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly ExternalReviewService _externalReviewService;
    private readonly MovieController _controller;

    public MovieControllerTests()
    {
        _mockCatalogService = new Mock<ICatalogService>();
        _mockReviewService = new Mock<IReviewService>();
        _mockCommentService = new Mock<ICommentService>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        
        // ExternalReviewService is sealed, so we use a real instance with no providers
        _externalReviewService = new ExternalReviewService(Enumerable.Empty<IExternalReviewProvider>());

        _mockCurrentUserService.Setup(s => s.CurrentUser).Returns(new CurrentUserDTO { Id = 1 });
        _mockCurrentUserService.Setup(s => s.InitializeAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _controller = new MovieController(
            _mockCatalogService.Object,
            _mockReviewService.Object,
            _mockCommentService.Object,
            _mockCurrentUserService.Object,
            _externalReviewService)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };
    }

    [Fact]
    public async Task Detail_ReturnsViewResult_WhenMovieExists()
    {
        // Arrange
        var movieId = 1;
        var movie = new Movie { Id = movieId, Title = "Test Movie", ReleaseYear = 2024 };
        _mockCatalogService.Setup(s => s.GetMovieByIdAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(movie);
        _mockReviewService.Setup(s => s.GetReviewsForMovieAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Review>());
        _mockCommentService.Setup(s => s.GetCommentsForMovieAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Comment>());

        // Act
        var result = await _controller.Detail(movieId);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<MovieDetailViewModel>().Subject;
        model.Movie.Should().Be(movie);
    }

    [Fact]
    public async Task Detail_ReturnsNotFoundView_WhenMovieDoesNotExist()
    {
        // Arrange
        var movieId = 999;
        _mockCatalogService.Setup(s => s.GetMovieByIdAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync((Movie)null);

        // Act
        var result = await _controller.Detail(movieId);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("NotFound");
    }

    [Fact]
    public async Task AddReview_RedirectsToDetail()
    {
        // Arrange
        var model = new AddReviewInputModel { MovieId = 1, StarRating = 5, Content = "Great!" };

        // Act
        var result = await _controller.AddReview(model);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Detail");
        redirectResult.RouteValues?["id"].Should().Be(1);
        _mockReviewService.Verify(s => s.AddReviewAsync(1, 1, 5, "Great!", It.IsAny<CancellationToken>()), Times.Once);
    }
}
