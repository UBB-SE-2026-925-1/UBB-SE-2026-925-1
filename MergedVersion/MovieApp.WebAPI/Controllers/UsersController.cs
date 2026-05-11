using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.DTOs;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using MovieApp.WebAPI.Controllers.DTOs;

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
    public async Task<ActionResult<CurrentUserDTO>> GetCurrentUser()
    {
        var user = await this.userRepository
            .FindByAuthIdentityAsync("Seed", "Admin");

        if (user == null)
        {
            return NotFound();
        }

        return Ok(new CurrentUserDTO
        {
            Id = user.Id,
            Username = user.Username,
            TotalPoints = user.UserStats?.TotalPoints ?? 0,
            WeeklyScore = user.UserStats?.WeeklyScore ?? 0
        });
    }

    [HttpGet("{userId}/stats")]
    public async Task<ActionResult<UserStatsDTO>> GetUserStats(int userId)
    {
        var stats = await this.pointService.GetUserStatsAsync(userId);

        return Ok(new UserStatsDTO
        {
            UserId = userId,
            TotalPoints = stats.TotalPoints,
            WeeklyScore = stats.WeeklyScore
        });
    }

    [HttpGet("badges")]
    public async Task<ActionResult<IEnumerable<Badge>>> GetAllBadges()
    {
        var badges = await this.badgeService.GetAllBadgesAsync();
        return Ok(badges);
    }

    [HttpGet("{userId}/badges")]
    public async Task<ActionResult<IEnumerable<UserBadgesDTO>>> GetUserBadges(int userId)
    {
        var badges = await this.badgeService.GetUserBadgesAsync(userId);
        return Ok(badges);
    }
}
