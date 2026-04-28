using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;

namespace MovieApp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        this.reviewService = reviewService;
    }

    [HttpGet("movie/{movieId}")]
    public async Task<ActionResult<IEnumerable<Review>>> GetReviewsForMovie(int movieId)
    {
        var reviews = await this.reviewService.GetReviewsForMovieAsync(movieId);
        return Ok(reviews);
    }

    [HttpPost]
    public async Task<ActionResult<Review>> AddReview([FromBody] AddReviewRequest request)
    {
        try
        {
            var review = await this.reviewService.AddReviewAsync(
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        await this.reviewService.DeleteReviewAsync(id);
        return NoContent();
    }

    [HttpGet("movie/{movieId}/average")]
    public async Task<ActionResult<double>> GetAverageRating(int movieId)
    {
        var average = await this.reviewService.GetAverageRatingAsync(movieId);
        return Ok(average);
    }

    public class AddReviewRequest
    {
        public int UserId { get; set; }
        public int MovieId { get; set; }
        public float Rating { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
