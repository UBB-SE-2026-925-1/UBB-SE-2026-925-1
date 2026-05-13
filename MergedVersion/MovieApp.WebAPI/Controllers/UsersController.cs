using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.DTOs;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using MovieApp.Infrastructure;
using MovieApp.Infrastructure.Data;
using MovieApp.WebAPI.Controllers.DTOs;

namespace MovieApp.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly MovieAppDbContext _context;
    private readonly IPointService _pointService;
    private readonly IBadgeService _badgeService;

    public UsersController(MovieAppDbContext context, IPointService pointService, IBadgeService badgeService)
    {
        _context = context;
        _pointService = pointService;
        _badgeService = badgeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _context.Users.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return user;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutUser(int id, User user)
    {
        if (id != user.Id)
        {
            return BadRequest();
        }

        _context.Entry(user).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserExists(id))
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
    public async Task<ActionResult<User>> PostUser(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetUser", new { id = user.Id }, user);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("current")]
    public async Task<ActionResult<CurrentUserDTO>> GetCurrentUser()
    {
        var user = await _context.Users
            .Include(u => u.UserStats)
            .FirstOrDefaultAsync(u => u.Username == "Admin");

        if (user == null)
        {
            return NotFound();
        }

        return new CurrentUserDTO
        {
            Id = user.Id,
            Username = user.Username,
            TotalPoints = user.UserStats?.TotalPoints ?? 0,
            WeeklyScore = user.UserStats?.WeeklyScore ?? 0
        };
    }

    [HttpGet("{userId}/stats")]
    public async Task<ActionResult<UserStatsDTO>> GetUserStats(int userId)
    {
        var stats = await _pointService.GetUserStatsAsync(userId);

        return new UserStatsDTO
        {
            UserId = userId,
            TotalPoints = stats.TotalPoints,
            WeeklyScore = stats.WeeklyScore
        };
    }

    [HttpGet("badges")]
    public async Task<ActionResult<IEnumerable<BadgeDTO>>> GetAllBadges()
    {
        var badges = await _badgeService.GetAllBadgesAsync();

        var result = badges.Select(b => new BadgeDTO
        {
            BadgeId = b.BadgeId,
            Name = b.Name,
            Description = b.Description,
            CriteriaValue = b.CriteriaValue
        });

        return Ok(result);
    }

    [HttpGet("{userId}/badges")]
    public async Task<ActionResult<IEnumerable<UserBadgesDTO>>> GetUserBadges(int userId)
    {
        var badges = await _badgeService.GetUserBadgesAsync(userId);
        return Ok(badges);
    }

    private bool UserExists(int id)
    {
        return _context.Users.Any(e => e.Id == id);
    }
}
