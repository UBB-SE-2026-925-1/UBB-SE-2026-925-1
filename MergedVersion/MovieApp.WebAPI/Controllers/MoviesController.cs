using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly ICatalogService catalogService;
    private readonly IMovieRepository movieRepository;

    public MoviesController(ICatalogService catalogService, IMovieRepository movieRepository)
    {
        this.catalogService = catalogService;
        this.movieRepository = movieRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Movie>>> GetMovies([FromQuery] string? query, [FromQuery] string? genres, [FromQuery] float? minRating)
    {
        if (!string.IsNullOrEmpty(query))
        {
            return await this.catalogService.SearchMoviesAsync(query);
        }

        if (!string.IsNullOrEmpty(genres) || minRating.HasValue)
        {
            var genreList = new List<Genre>();
            if (!string.IsNullOrEmpty(genres))
            {
                var genreNames = genres.Split(',');
                var allGenres = await this.movieRepository.GetGenresAsync();
                genreList = allGenres.Where(g => genreNames.Contains(g.Name)).ToList();
            }

            return await this.catalogService.FilterMoviesAsync(genreList, minRating ?? 0f);
        }

        return await this.catalogService.GetAllMoviesAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Movie>> GetMovie(int id)
    {
        var movie = await this.catalogService.GetMovieByIdAsync(id);
        if (movie == null)
        {
            return NotFound();
        }

        return movie;
    }

    [HttpGet("genres")]
    public async Task<ActionResult<IEnumerable<Genre>>> GetGenres()
    {
        var genres = await this.movieRepository.GetGenresAsync();
        return Ok(genres);
    }
}
