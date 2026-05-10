using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    /// <summary>Smallest valid auto-generated comment identifier in the database. Used to distinguish a real parent reference from "not set".</summary>
    private const int MinimumValidCommentId = 1;

    private readonly ICatalogService catalogService;
    private readonly IMovieRepository movieRepository;
    private readonly IReviewService reviewService;
    private readonly ICommentService commentService;

    public MoviesController(
        ICatalogService catalogService,
        IMovieRepository movieRepository,
        IReviewService reviewService,
        ICommentService commentService)
    {
        this.catalogService = catalogService;
        this.movieRepository = movieRepository;
        this.reviewService = reviewService;
        this.commentService = commentService;
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

    // ---- Reviews (nested routes per debugging plan: /api/movies/{movieId}/reviews) ----

    [HttpGet("{movieId}/reviews")]
    public async Task<ActionResult<IEnumerable<Review>>> GetReviewsForMovie(int movieId)
    {
        var reviewsForMovie = await this.reviewService.GetReviewsForMovieAsync(movieId);
        return Ok(reviewsForMovie);
    }

    [HttpPost("{movieId}/reviews")]
    public async Task<ActionResult<Review>> AddReviewForMovie(int movieId, [FromBody] AddReviewRequest request)
    {
        try
        {
            var createdReview = await this.reviewService.AddReviewAsync(request.UserId, movieId, request.Rating, request.Content);
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
        var averageStarRating = await this.reviewService.GetAverageRatingAsync(movieId);
        return Ok(averageStarRating);
    }

    // ---- Comments (nested routes per debugging plan: /api/movies/{movieId}/comments) ----

    [HttpGet("{movieId}/comments")]
    public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsForMovie(int movieId)
    {
        var commentsForMovie = await this.commentService.GetCommentsForMovieAsync(movieId);
        return Ok(commentsForMovie);
    }

    [HttpPost("{movieId}/comments")]
    public async Task<ActionResult<Comment>> AddCommentForMovie(int movieId, [FromBody] AddCommentRequest request)
    {
        try
        {
            // ParentCommentId is optional — when present and well-formed we route through AddReplyAsync to support threaded replies.
            bool isReplyToExistingComment =
                request.ParentCommentId.HasValue &&
                request.ParentCommentId.Value >= MinimumValidCommentId;

            Comment createdComment = isReplyToExistingComment
                ? await this.commentService.AddReplyAsync(request.UserId, request.ParentCommentId!.Value, request.Content)
                : await this.commentService.AddCommentAsync(request.UserId, movieId, request.Content);

            return Ok(createdComment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
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
