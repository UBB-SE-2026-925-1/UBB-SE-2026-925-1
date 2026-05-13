using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Infrastructure;
using MovieApp.Infrastructure.Data;

namespace MovieApp.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MoviesController : ControllerBase
{
    private const int MinimumValidCommentId = 1;

    private readonly MovieAppDbContext _context;
    private readonly ICatalogService _catalogService;
    private readonly IMovieRepository _movieRepository;
    private readonly IReviewService _reviewService;
    private readonly ICommentService _commentService;

    public MoviesController(
        MovieAppDbContext context,
        ICatalogService catalogService,
        IMovieRepository movieRepository,
        IReviewService reviewService,
        ICommentService commentService)
    {
        _context = context;
        _catalogService = catalogService;
        _movieRepository = movieRepository;
        _reviewService = reviewService;
        _commentService = commentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Movie>>> GetMovies([FromQuery] string? query, [FromQuery] string? genres, [FromQuery] float? minRating)
    {
        if (!string.IsNullOrEmpty(query))
        {
            return await _catalogService.SearchMoviesAsync(query);
        }

        if (!string.IsNullOrEmpty(genres) || minRating.HasValue)
        {
            var genreList = new List<Genre>();
            if (!string.IsNullOrEmpty(genres))
            {
                var genreNames = genres.Split(',');
                var allGenres = await _movieRepository.GetGenresAsync();
                genreList = allGenres.Where(g => genreNames.Contains(g.Name)).ToList();
            }

            return await _catalogService.FilterMoviesAsync(genreList, minRating ?? 0f);
        }

        return await _context.Movies.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Movie>> GetMovie(int id)
    {
        var movie = await _context.Movies.FindAsync(id);

        if (movie == null)
        {
            return NotFound();
        }

        return movie;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutMovie(int id, Movie movie)
    {
        if (id != movie.Id)
        {
            return BadRequest();
        }

        _context.Entry(movie).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MovieExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<Movie>> PostMovie(Movie movie)
    {
        _context.Movies.Add(movie);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetMovie", new { id = movie.Id }, movie);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMovie(int id)
    {
        var movie = await _context.Movies.FindAsync(id);
        if (movie == null)
        {
            return NotFound();
        }

        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("genres")]
    public async Task<ActionResult<IEnumerable<Genre>>> GetGenres()
    {
        return await _context.Genres.ToListAsync();
    }


    [HttpGet("{movieId}/reviews")]
    public async Task<ActionResult<IEnumerable<Review>>> GetReviewsForMovie(int movieId)
    {
        return await _context.Reviews
            .Where(r => r.Movie != null && r.Movie.Id == movieId)
            .ToListAsync();
    }

    [HttpPost("{movieId}/reviews")]
    public async Task<ActionResult<Review>> AddReviewForMovie(int movieId, [FromBody] AddReviewRequest request)
    {
        try
        {
            var createdReview = await _reviewService.AddReviewAsync(request.UserId, movieId, request.Rating, request.Content);
            return Ok(createdReview);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{movieId}/reviews/average")]
    public async Task<ActionResult<double>> GetAverageRatingForMovie(int movieId)
    {
        var averageStarRating = await _reviewService.GetAverageRatingAsync(movieId);
        return Ok(averageStarRating);
    }


    [HttpGet("{movieId}/comments")]
    public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsForMovie(int movieId)
    {
        return await _context.Comments
            .Where(c => c.MovieId == movieId)
            .ToListAsync();
    }

    [HttpPost("{movieId}/comments")]
    public async Task<ActionResult<Comment>> AddCommentForMovie(int movieId, [FromBody] AddCommentRequest request)
    {
        try
        {
            bool isReplyToExistingComment =
                request.ParentCommentId.HasValue &&
                request.ParentCommentId.Value >= MinimumValidCommentId;

            Comment createdComment = isReplyToExistingComment
                ? await _commentService.AddReplyAsync(request.UserId, request.ParentCommentId!.Value, request.Content)
                : await _commentService.AddCommentAsync(request.UserId, movieId, request.Content);

            return Ok(createdComment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private bool MovieExists(int id)
    {
        return _context.Movies.Any(e => e.Id == id);
    }

    public class AddReviewRequest
    {
        public int UserId { get; set; }
        public float Rating { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    public class AddCommentRequest
    {
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int? ParentCommentId { get; set; }
    }
}
