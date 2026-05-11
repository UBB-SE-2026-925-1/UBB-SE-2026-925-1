#nullable enable
using Moq;
using MovieApp.Core.Repositories;
using MovieApp.Core.Models;
using MovieApp.Core.Services;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieApp.Core.Tests;

public class CatalogServiceTests
{
    private readonly Mock<IMovieRepository> movieRepoMock;
    private readonly CatalogService sut;

    public CatalogServiceTests()
    {
        movieRepoMock = new Mock<IMovieRepository>();
        sut = new CatalogService(movieRepoMock.Object);
    }

    // --- GetAllMovies ---
    [Fact]
    public async Task GetAllMovies_WhenMoviesExist_ReturnsMoviesOrderedByTitle()
    {
        var movies = new List<Movie>
        {
            new () { Id = 1, Title = "Zoolander", AverageRating = 3.5 },
            new () { Id = 2, Title = "Inception", AverageRating = 4.8 },
            new () { Id = 3, Title = "Avatar", AverageRating = 4.0 }
        };
        movieRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(movies);

        var result = await sut.GetAllMoviesAsync();

        Assert.Equal(3, result.Count);
        Assert.Equal("Avatar", result[0].Title);
        Assert.Equal("Inception", result[1].Title);
        Assert.Equal("Zoolander", result[2].Title);
    }

    [Fact]
    public async Task GetAllMovies_WhenNoMoviesExist_ReturnsEmptyList()
    {
        movieRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Movie>());

        var result = await sut.GetAllMoviesAsync();

        Assert.Empty(result);
    }

    // --- GetMovieById ---
    [Fact]
    public async Task GetMovieById_WhenMovieExists_ReturnsMovie()
    {
        var movie = new Movie { Id = 42, Title = "Inception" };
        movieRepoMock.Setup(r => r.GetByIdAsync(42, default)).ReturnsAsync(movie);

        var result = await sut.GetMovieByIdAsync(42);

        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
        Assert.Equal("Inception", result.Title);
    }

    [Fact]
    public async Task GetMovieById_WhenMovieNotFound_ReturnsNull()
    {
        movieRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), default)).ReturnsAsync((Movie?)null);

        var result = await sut.GetMovieByIdAsync(999);

        Assert.Null(result);
    }

    // --- SearchMovies ---
    [Fact]
    public async Task SearchMovies_WithMatchingQuery_ReturnsOnlyMatchingMovies()
    {
        var movies = new List<Movie>
        {
            new () { Id = 1, Title = "Inception" },
            new () { Id = 2, Title = "Interstellar" },
            new () { Id = 3, Title = "The Dark Knight" }
        };
        movieRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(movies);

        var result = await sut.SearchMoviesAsync("in");

        Assert.Equal(2, result.Count);
        Assert.All(result, m => Assert.Contains("in", m.Title, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchMovies_WithEmptyQuery_ReturnsAllMovies()
    {
        var movies = new List<Movie>
        {
            new () { Id = 1, Title = "Inception" },
            new () { Id = 2, Title = "Avatar" }
        };
        movieRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(movies);

        var result = await sut.SearchMoviesAsync(string.Empty);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task SearchMovies_WithCaseMismatch_ReturnsMatchingMovies()
    {
        var movies = new List<Movie>
        {
            new () { Id = 1, Title = "INCEPTION" },
            new () { Id = 2, Title = "Avatar" }
        };
        movieRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(movies);

        var result = await sut.SearchMoviesAsync("inception");

        Assert.Single(result);
        Assert.Equal("INCEPTION", result[0].Title);
    }

    [Fact]
    public async Task SearchMovies_WithQueryMatchingNoMovies_ReturnsEmptyList()
    {
        var movies = new List<Movie>
        {
            new () { Id = 1, Title = "Inception" }
        };
        movieRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(movies);

        var result = await sut.SearchMoviesAsync("zzz");

        Assert.Empty(result);
    }

    // --- FilterMovies ---
    [Fact]
    public async Task FilterMovies_WithGenreAndMinRating_ReturnsOnlyMatchingMovies()
    {
        var movies = new List<Movie>
        {
            new () { Id = 1, Title = "A", Genres = new List<Genre> { new Genre { Name = "Comedy" } }, AverageRating = 4.0 },
            new () { Id = 2, Title = "B", Genres = new List<Genre> { new Genre { Name = "Comedy" } }, AverageRating = 2.0 },
            new () { Id = 3, Title = "C", Genres = new List<Genre> { new Genre { Name = "Drama" } }, AverageRating = 4.5 }
        };
        movieRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(movies);

        var result = await sut.FilterMoviesAsync(new List<Genre> { new Genre { Name = "Comedy" } }, 3.0f);

        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }

    [Fact]
    public async Task FilterMovies_WithNoGenreFilter_ReturnsAllMoviesAboveMinRating()
    {
        var movies = new List<Movie>
        {
            new () { Id = 1, Title = "A", Genres = new List<Genre> { new Genre { Name = "Comedy" } }, AverageRating = 4.0 },
            new () { Id = 2, Title = "B", Genres = new List<Genre> { new Genre { Name = "Drama" } }, AverageRating = 2.0 }
        };
        movieRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(movies);

        var result = await sut.FilterMoviesAsync(new List<Genre>(), 3.0f);

        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }

    [Fact]
    public async Task FilterMovies_WithCaseInsensitiveGenre_ReturnsMatchingMovies()
    {
        var movies = new List<Movie>
        {
            new () { Id = 1, Title = "A", Genres = new List<Genre> { new Genre { Name = "COMEDY" } }, AverageRating = 4.0 }
        };
        movieRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(movies);

        var result = await sut.FilterMoviesAsync(new List<Genre> { new Genre { Name = "comedy" } }, 0f);

        Assert.Single(result);
    }
}
