using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Infrastructure;
using MovieApp.Infrastructure.Data;

namespace MovieApp.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CommentsController : ControllerBase
{
    private readonly MovieAppDbContext _context;
    private readonly ICommentService _commentService;

    public CommentsController(MovieAppDbContext context, ICommentService commentService)
    {
        _context = context;
        _commentService = commentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Comment>>> GetComments()
    {
        return await _context.Comments.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Comment>> GetComment(int id)
    {
        var comment = await _context.Comments.FindAsync(id);

        if (comment == null)
        {
            return NotFound();
        }

        return comment;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutComment(int id, Comment comment)
    {
        if (id != comment.MessageId)
        {
            return BadRequest();
        }

        _context.Entry(comment).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CommentExists(id))
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
    public async Task<ActionResult<Comment>> PostComment(Comment comment)
    {
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetComment", new { id = comment.MessageId }, comment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment == null)
        {
            return NotFound();
        }

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("movie/{movieId}")]
    public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsForMovie(int movieId)
    {
        return await _context.Comments
            .Where(c => c.MovieId == movieId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    [HttpPost("add")]
    public async Task<ActionResult<Comment>> AddComment([FromBody] AddCommentRequest request)
    {
        try
        {
            var comment = await _commentService.AddCommentAsync(
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
            var comment = await _commentService.AddReplyAsync(
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

    private bool CommentExists(int id)
    {
        return _context.Comments.Any(e => e.MessageId == id);
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
