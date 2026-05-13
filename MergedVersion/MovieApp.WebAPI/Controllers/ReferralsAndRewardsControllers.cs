using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using MovieApp.Infrastructure;
using MovieApp.Infrastructure.Data;

namespace MovieApp.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReferralsController : ControllerBase
{
    private readonly MovieAppDbContext _context;
    private readonly IReferralValidator _referralValidator;
    private readonly IReferralLogService _referralLogService;

    public ReferralsController(
        MovieAppDbContext context,
        IReferralValidator referralValidator, 
        IReferralLogService referralLogService)
    {
        _context = context;
        _referralValidator = referralValidator;
        _referralLogService = referralLogService;
    }

    [HttpGet("validate")]
    public async Task<ActionResult<bool>> ValidateCode([FromQuery] string code)
    {
        var isValid = await _referralValidator.IsValidReferralAsync(code, 0);
        return Ok(isValid);
    }

    [HttpGet("user/{userId}/code")]
    public async Task<ActionResult<string>> GetCode(int userId)
    {
        var profile = await _context.AmbassadorProfiles
            .FirstOrDefaultAsync(ap => ap.UserId == userId);
        return Ok(profile?.PermanentCode);
    }

    [HttpPost("profile")]
    public async Task<IActionResult> CreateProfile([FromBody] CreateProfileRequest request)
    {
        var profile = new AmbassadorProfile
        {
            UserId = request.UserId,
            PermanentCode = request.Code,
            RewardBalance = 0
        };
        _context.AmbassadorProfiles.Add(profile);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("code/{code}/user")]
    public async Task<ActionResult<int?>> GetUserIdByCode(string code)
    {
        var profile = await _context.AmbassadorProfiles
            .FirstOrDefaultAsync(ap => ap.PermanentCode == code);
        return Ok(profile?.UserId);
    }

    [HttpPost("log")]
    public async Task<IActionResult> AddLog([FromBody] AddLogRequest request)
    {
        var log = new ReferralLog
        {
            AmbassadorId = request.AmbassadorId,
            ReferredUserId = request.FriendId,
            EventId = request.EventId,
            CreatedAt = DateTime.UtcNow
        };
        _context.ReferralLogs.Add(log);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("reward/apply")]
    public async Task<ActionResult<bool>> ApplyReward([FromBody] ApplyRewardRequest request)
    {
        var profile = await _context.AmbassadorProfiles
            .FirstOrDefaultAsync(ap => ap.UserId == request.AmbassadorId);

        if (profile == null) return false;

        profile.RewardBalance++;
        await _context.SaveChangesAsync();
        return Ok(true);
    }

    [HttpGet("user/{userId}/history")]
    public async Task<ActionResult<IEnumerable<ReferralHistoryItem>>> GetHistory(int userId)
    {
        var history = await _context.ReferralLogs
            .Where(rl => rl.AmbassadorId == userId)
            .Select(rl => new ReferralHistoryItem
            {
                FriendName = rl.ReferredUser != null ? rl.ReferredUser.Username : "Unknown",
                EventTitle = rl.Event != null ? rl.Event.Title : "Deleted Event",
                UsedAt = rl.CreatedAt
            })
            .ToListAsync();
        return Ok(history);
    }

    [HttpGet("user/{userId}/balance")]
    public async Task<ActionResult<int>> GetBalance(int userId)
    {
        var profile = await _context.AmbassadorProfiles
            .FirstOrDefaultAsync(ap => ap.UserId == userId);
        return Ok(profile?.RewardBalance ?? 0);
    }

    [HttpPost("user/{userId}/balance/decrement")]
    public async Task<IActionResult> DecrementBalance(int userId)
    {
        var profile = await _context.AmbassadorProfiles
            .FirstOrDefaultAsync(ap => ap.UserId == userId);

        if (profile != null && profile.RewardBalance > 0)
        {
            profile.RewardBalance--;
            await _context.SaveChangesAsync();
        }
        return Ok();
    }

    [HttpGet("check")]
    public async Task<ActionResult<bool>> CheckLog([FromQuery] int ambassadorId, [FromQuery] int friendId, [FromQuery] int eventId)
    {
        var exists = await _context.ReferralLogs
            .AnyAsync(rl => rl.AmbassadorId == ambassadorId &&
                            rl.ReferredUserId == friendId &&
                            rl.EventId == eventId);
        return Ok(exists);
    }

    public class CreateProfileRequest { public int UserId { get; set; } public string Code { get; set; } = string.Empty; }
    public class AddLogRequest { public int AmbassadorId { get; set; } public int FriendId { get; set; } public int EventId { get; set; } }
    public class ApplyRewardRequest { public int AmbassadorId { get; set; } }
}

[Route("api/[controller]")]
[ApiController]
public class RewardsController : ControllerBase
{
    private readonly MovieAppDbContext _context;
    private readonly ISlotMachineService _slotMachineService;

    public RewardsController(MovieAppDbContext context, ISlotMachineService slotMachineService)
    {
        _context = context;
        _slotMachineService = slotMachineService;
    }

    [HttpPost("claim/{userId}")]
    public async Task<IActionResult> ClaimDailyReward(int userId)
    {
        try
        {
            await _slotMachineService.RecordLoginAndCheckStreakAsync(userId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }



}
