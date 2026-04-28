using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;

namespace MovieApp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TriviaController : ControllerBase
{
    private readonly ITriviaRepository triviaRepository;
    private readonly ITriviaRewardRepository rewardRepository;

    public TriviaController(ITriviaRepository triviaRepository, ITriviaRewardRepository rewardRepository)
    {
        this.triviaRepository = triviaRepository;
        this.rewardRepository = rewardRepository;
    }

    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<TriviaQuestion>>> GetByCategory(string category)
    {
        var questions = await this.triviaRepository.GetByCategoryAsync(category);
        return Ok(questions);
    }

    [HttpGet("movie/{movieId}")]
    public async Task<ActionResult<IEnumerable<TriviaQuestion>>> GetByMovie(int movieId, [FromQuery] int count = 5)
    {
        var questions = await this.triviaRepository.GetByMovieIdAsync(movieId, count);
        return Ok(questions);
    }

    [HttpGet("reward/{userId}")]
    public async Task<ActionResult<TriviaReward>> GetUnredeemedReward(int userId)
    {
        var reward = await this.rewardRepository.GetUnredeemedByUserAsync(userId);
        if (reward == null) return NotFound();
        return Ok(reward);
    }
}

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        this.notificationService = notificationService;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Notification>>> GetUserNotifications(int userId)
    {
        var notifications = await this.notificationService.GetNotificationsByUserIdAsync(userId);
        return Ok(notifications);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveNotification(int id)
    {
        await this.notificationService.RemoveNotificationAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await this.notificationService.MarkAsReadOrRemoveAsync(id);
        return Ok();
    }
}
