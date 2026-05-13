using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
namespace MovieApp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarathonsController : ControllerBase
{
    private readonly IMarathonService marathonService;
    private readonly ICurrentUserService currentUserService;

    public MarathonsController(IMarathonService marathonService, ICurrentUserService currentUserService)
    {
        this.marathonService = marathonService;
        this.currentUserService = currentUserService;
    }

    [HttpGet("weekly/{userId}")]
    public async Task<ActionResult<IEnumerable<Marathon>>> GetWeeklyMarathons(int userId)
    {
        var marathons = await this.marathonService.GetWeeklyMarathonsAsync(userId);
        return Ok(marathons);
    }

    [HttpGet("{id}/movies")]
    public async Task<ActionResult<IEnumerable<Movie>>> GetMarathonMovies(int id)
    {
        var movies = await this.marathonService.GetMoviesForMarathonAsync(id);
        return Ok(movies);
    }

    [HttpGet("{id}/progress/{userId}")]
    public async Task<ActionResult<MarathonProgress>> GetProgress(int id, int userId)
    {
        var progress = await this.marathonService.GetUserProgressAsync(userId, id);
        if (progress == null) return NotFound();
        return Ok(progress);
    }

    [HttpPost("{id}/start")]
    public async Task<ActionResult<bool>> StartMarathon(int id)
    {
        await this.currentUserService.InitializeAsync();
        var success = await this.marathonService.StartMarathonAsync(id);
        return Ok(success);
    }

    [HttpGet("{id}/leaderboard")]
    public async Task<ActionResult<IEnumerable<LeaderboardEntry>>> GetLeaderboard(int id)
    {
        var leaderboard = await this.marathonService.GetLeaderboardWithUsernamesAsync(id);
        return Ok(leaderboard);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Marathon>>> GetMarathons([FromServices] IMarathonRepository marathonRepository)
    {
        var marathons = await marathonRepository.GetActiveMarathonsAsync();
        return Ok(marathons);
    }

    [HttpPost("{id}/quiz")]
    public async Task<ActionResult> UpdateQuizResult(int id, [FromBody] int correctAnswersCount)
    {
        await this.currentUserService.InitializeAsync();
        await this.marathonService.UpdateQuizResultAsync(id, correctAnswersCount);
        return Ok();
    }

    [HttpPost("{id}/movies/{movieId}/log")]
    public async Task<ActionResult<bool>> LogMovie(int id, int movieId, [FromBody] int correctAnswersCount)
    {
        await this.currentUserService.InitializeAsync();
        var success = await this.marathonService.LogMovieAsync(id, movieId, correctAnswersCount);
        return Ok(success);
    }

    [HttpGet("{id}/participants/count")]
    public async Task<ActionResult<int>> GetParticipantCount(int id)
    {
        var count = await this.marathonService.GetParticipantCountAsync(id);
        return Ok(count);
    }

    [HttpGet("{id}/movies/count")]
    public async Task<ActionResult<int>> GetMarathonMovieCount(int id)
    {
        var count = await this.marathonService.GetMarathonMovieCountAsync(id);
        return Ok(count);
    }

    [HttpGet("{id}/prerequisite/{userId}")]
    public async Task<ActionResult<bool>> IsPrerequisiteCompleted(int id, int userId)
    {
        var isCompleted = await this.marathonService.IsPrerequisiteCompletedAsync(userId, id);
        return Ok(isCompleted);
    }
}
