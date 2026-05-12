using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Proxy;
using MovieApp.Web.Controllers;
using MovieApp.Web.Models;
using System.Net;
using System.Net.Http.Json;

namespace MovieApp.Web.Tests.Controllers;

public class CatalogControllerTests
{
    private readonly Mock<ICatalogService> _mockCatalogService;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly CatalogController _controller;

    public CatalogControllerTests()
    {
        _mockCatalogService = new Mock<ICatalogService>();
        _mockCache = new Mock<IMemoryCache>();

        // Mocking IMemoryCache.GetOrCreateAsync is tricky, 
        // so we'll just mock ICacheEntry and the CreateEntry method.
        var cacheEntry = new Mock<ICacheEntry>();
        _mockCache.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(cacheEntry.Object);

        // We need a real ApiClient with a mocked handler to test CatalogController
        var handlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/") };
        var apiClient = new ApiClient(httpClient);

        _controller = new CatalogController(_mockCatalogService.Object, apiClient, _mockCache.Object);
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithAllMovies_WhenNoFilters()
    {
        // Arrange
        var movies = new List<Movie> { new Movie { Id = 1, Title = "Movie 1" } };
        _mockCatalogService.Setup(s => s.GetAllMoviesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(movies);

        // Act
        var result = await _controller.Index(null, null);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<CatalogViewModel>().Subject;
        model.Movies.Should().HaveCount(1);
        model.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Index_CallsSearch_WhenQueryProvided()
    {
        // Arrange
        var q = "Avatar";
        _mockCatalogService.Setup(s => s.SearchMoviesAsync(q, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Movie>());

        // Act
        await _controller.Index(q, null);

        // Assert
        _mockCatalogService.Verify(s => s.SearchMoviesAsync(q, It.IsAny<CancellationToken>()), Times.Once);
    }
}
