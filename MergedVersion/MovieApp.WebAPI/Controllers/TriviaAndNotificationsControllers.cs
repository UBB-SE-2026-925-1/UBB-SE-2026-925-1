using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;
using MovieApp.Core.Services;
using MovieApp.Infrastructure;
using MovieApp.Infrastructure.Data;

namespace MovieApp.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TriviaController : ControllerBase
{
    private readonly MovieAppDbContext _context;

    public TriviaController(MovieAppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TriviaQuestion>>> GetTriviaQuestions()
    {
        return await _context.TriviaQuestions.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TriviaQuestion>> GetTriviaQuestion(int id)
    {
        var triviaQuestion = await _context.TriviaQuestions.FindAsync(id);

        if (triviaQuestion == null)
        {
            return NotFound();
        }

        return triviaQuestion;
    }

    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<TriviaQuestion>>> GetByCategory(string category)
    {
        return await _context.TriviaQuestions
            .Where(q => q.Category == category)
            .ToListAsync();
    }

    [HttpGet("movie/{movieId}")]
    public async Task<ActionResult<IEnumerable<TriviaQuestion>>> GetByMovie(int movieId, [FromQuery] int count = 5)
    {
        return await _context.TriviaQuestions
            .Where(q => q.MovieId == movieId)
            .Take(count)
            .ToListAsync();
    }

    [HttpGet("reward/{userId}")]
    public async Task<ActionResult<TriviaReward>> GetUnredeemedReward(int userId)
    {
        var reward = await _context.TriviaRewards
            .FirstOrDefaultAsync(r => r.UserId == userId && !r.IsRedeemed);
        if (reward == null) return NotFound();
        return reward;
    }
}

[Route("api/[controller]")]
[ApiController]
public class NotificationsController : ControllerBase
{
    private readonly MovieAppDbContext _context;
    private readonly INotificationService _notificationService;

    public NotificationsController(MovieAppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications()
    {
        return await _context.Notifications.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Notification>> GetNotification(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);

        if (notification == null)
        {
            return NotFound();
        }

        return notification;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Notification>>> GetUserNotifications(int userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .ToListAsync();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null)
        {
            return NotFound();
        }

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification != null)
        {
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }
        return Ok();
    }
}
