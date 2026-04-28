using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarathonsController : ControllerBase
{
    private readonly IMarathonService marathonService;

    public MarathonsController(IMarathonService marathonService)
    {
        this.marathonService = marathonService;
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
        var success = await this.marathonService.StartMarathonAsync(id);
        return Ok(success);
    }

    [HttpGet("{id}/leaderboard")]
    public async Task<ActionResult<IEnumerable<LeaderboardEntry>>> GetLeaderboard(int id)
    {
        var leaderboard = await this.marathonService.GetLeaderboardWithUsernamesAsync(id);
        return Ok(leaderboard);
    }
}
