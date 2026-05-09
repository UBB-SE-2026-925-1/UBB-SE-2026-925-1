using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;

namespace MovieApp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IPointService pointService;
    private readonly IBadgeService badgeService;
    private readonly IUserRepository userRepository;

    public UsersController(IPointService pointService, IBadgeService badgeService, IUserRepository userRepository)
    {
        this.pointService = pointService;
        this.badgeService = badgeService;
        this.userRepository = userRepository;
    }

    [HttpGet("current")]
    public async Task<ActionResult<User>> GetCurrentUser()
    {
        var user = await this.userRepository.FindByAuthIdentityAsync("Seed", "Admin");
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpGet("{id}/stats")]
    public async Task<ActionResult<UserStats>> GetUserStats(int id)
    {
        var stats = await this.pointService.GetUserStatsAsync(id);
        return Ok(stats);
    }

    [HttpGet("badges")]
    public async Task<ActionResult<IEnumerable<Badge>>> GetAllBadges()
    {
        var badges = await this.badgeService.GetAllBadgesAsync();
        return Ok(badges);
    }

    [HttpGet("{id}/badges")]
    public async Task<ActionResult<IEnumerable<Badge>>> GetUserBadges(int id)
    {
        var badges = await this.badgeService.GetUserBadgesAsync(id);
        return Ok(badges);
    }
}
