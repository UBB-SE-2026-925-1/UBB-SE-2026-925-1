using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Infrastructure;
using MovieApp.Infrastructure.Data;

namespace MovieApp.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReviewsController : ControllerBase
{
    private readonly MovieAppDbContext _context;
    private readonly IReviewService _reviewService;

    public ReviewsController(MovieAppDbContext context, IReviewService reviewService)
    {
        _context = context;
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Review>>> GetReviews()
    {
        return await _context.Reviews.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Review>> GetReview(int id)
    {
        var review = await _context.Reviews.FindAsync(id);

        if (review == null)
        {
            return NotFound();
        }

        return review;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutReview(int id, Review review)
    {
        if (id != review.ReviewId)
        {
            return BadRequest();
        }

        _context.Entry(review).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ReviewExists(id))
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
    public async Task<ActionResult<Review>> PostReview(Review review)
    {
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetReview", new { id = review.ReviewId }, review);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
        {
            return NotFound();
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("movie/{movieId}")]
    public async Task<ActionResult<IEnumerable<Review>>> GetReviewsForMovie(int movieId)
    {
        return await _context.Reviews
            .Where(r => r.Movie != null && r.Movie.Id == movieId && r.StarRating <= 5)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    [HttpPost("add")]
    public async Task<ActionResult<Review>> AddReview([FromBody] AddReviewRequest request)
    {
        try
        {
            var review = await _reviewService.AddReviewAsync(
                request.UserId,
                request.MovieId,
                request.Rating,
                request.Content);
            return Ok(review);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("movie/{movieId}/average")]
    public async Task<ActionResult<double>> GetAverageRating(int movieId)
    {
        var average = await _reviewService.GetAverageRatingAsync(movieId);
        return Ok(average);
    }

    private bool ReviewExists(int id)
    {
        return _context.Reviews.Any(e => e.ReviewId == id);
    }

    public class AddReviewRequest
    {
        public int UserId { get; set; }
        public int MovieId { get; set; }
        public float Rating { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
