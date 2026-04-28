using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;

namespace MovieApp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService commentService;

    public CommentsController(ICommentService commentService)
    {
        this.commentService = commentService;
    }

    [HttpGet("movie/{movieId}")]
    public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsForMovie(int movieId)
    {
        var comments = await this.commentService.GetCommentsForMovieAsync(movieId);
        return Ok(comments);
    }

    [HttpPost]
    public async Task<ActionResult<Comment>> AddComment([FromBody] AddCommentRequest request)
    {
        try
        {
            var comment = await this.commentService.AddCommentAsync(
                request.UserId,
                request.MovieId,
                request.Content);
            return Ok(comment);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("reply")]
    public async Task<ActionResult<Comment>> AddReply([FromBody] AddReplyRequest request)
    {
        try
        {
            var comment = await this.commentService.AddReplyAsync(
                request.UserId,
                request.ParentCommentId,
                request.Content);
            return Ok(comment);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        await this.commentService.DeleteCommentAsync(id);
        return NoContent();
    }

    public class AddCommentRequest
    {
        public int UserId { get; set; }
        public int MovieId { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    public class AddReplyRequest
    {
        public int UserId { get; set; }
        public int ParentCommentId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
