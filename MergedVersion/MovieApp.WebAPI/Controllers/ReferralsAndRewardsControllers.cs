using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Models;
using MovieApp.Core.Services;
using MovieApp.Core.Repositories;

namespace MovieApp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReferralsController : ControllerBase
{
    private readonly IReferralValidator referralValidator;
    private readonly IReferralLogService referralLogService;
    private readonly IAmbassadorRepository ambassadorRepository;

    public ReferralsController(
        IReferralValidator referralValidator, 
        IReferralLogService referralLogService,
        IAmbassadorRepository ambassadorRepository)
    {
        this.referralValidator = referralValidator;
        this.referralLogService = referralLogService;
        this.ambassadorRepository = ambassadorRepository;
    }

    [HttpGet("validate")]
    public async Task<ActionResult<bool>> ValidateCode([FromQuery] string code)
    {
        var isValid = await this.referralValidator.IsValidReferralAsync(code, 0);
        return Ok(isValid);
    }

    [HttpGet("user/{userId}/code")]
    public async Task<ActionResult<string>> GetCode(int userId)
    {
        var code = await this.ambassadorRepository.GetReferralCodeAsync(userId);
        return Ok(code);
    }

    [HttpPost("profile")]
    public async Task<IActionResult> CreateProfile([FromBody] CreateProfileRequest request)
    {
        await this.ambassadorRepository.CreateAmbassadorProfileAsync(request.UserId, request.Code);
        return Ok();
    }

    [HttpGet("code/{code}/user")]
    public async Task<ActionResult<int?>> GetUserIdByCode(string code)
    {
        var userId = await this.ambassadorRepository.GetUserIdByReferralCodeAsync(code);
        return Ok(userId);
    }

    [HttpPost("log")]
    public async Task<IActionResult> AddLog([FromBody] AddLogRequest request)
    {
        await this.ambassadorRepository.AddReferralLogAsync(request.AmbassadorId, request.FriendId, request.EventId);
        return Ok();
    }

    [HttpPost("reward/apply")]
    public async Task<ActionResult<bool>> ApplyReward([FromBody] ApplyRewardRequest request)
    {
        var success = await this.ambassadorRepository.TryApplyRewardAsync(request.AmbassadorId);
        return Ok(success);
    }

    [HttpGet("user/{userId}/history")]
    public async Task<ActionResult<IEnumerable<ReferralHistoryItem>>> GetHistory(int userId)
    {
        var history = await this.ambassadorRepository.GetReferralHistoryAsync(userId);
        return Ok(history);
    }

    [HttpGet("user/{userId}/balance")]
    public async Task<ActionResult<int>> GetBalance(int userId)
    {
        var balance = await this.ambassadorRepository.GetRewardBalanceAsync(userId);
        return Ok(balance);
    }

    [HttpPost("user/{userId}/balance/decrement")]
    public async Task<IActionResult> DecrementBalance(int userId)
    {
        await this.ambassadorRepository.DecrementRewardBalanceAsync(userId);
        return Ok();
    }

    [HttpGet("check")]
    public async Task<ActionResult<bool>> CheckLog([FromQuery] int ambassadorId, [FromQuery] int friendId, [FromQuery] int eventId)
    {
        var exists = await this.ambassadorRepository.HasReferralLogAsync(ambassadorId, friendId, eventId);
        return Ok(exists);
    }

    public class CreateProfileRequest { public int UserId { get; set; } public string Code { get; set; } = string.Empty; }
    public class AddLogRequest { public int AmbassadorId { get; set; } public int FriendId { get; set; } public int EventId { get; set; } }
    public class ApplyRewardRequest { public int AmbassadorId { get; set; } }
}

[ApiController]
[Route("api/[controller]")]
public class RewardsController : ControllerBase
{
    private readonly ISlotMachineService slotMachineService;
    private readonly IUserMovieDiscountRepository discountRepository;

    public RewardsController(ISlotMachineService slotMachineService, IUserMovieDiscountRepository discountRepository)
    {
        this.slotMachineService = slotMachineService;
        this.discountRepository = discountRepository;
    }

    [HttpPost("claim/{userId}")]
    public async Task<IActionResult> ClaimDailyReward(int userId)
    {
        try
        {
            await this.slotMachineService.RecordLoginAndCheckStreakAsync(userId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Reward>>> GetUserRewards(int userId)
    {
        var rewards = await this.discountRepository.GetDiscountsForUserAsync(userId);
        return Ok(rewards);
    }

    [HttpPost("{id}/redeem")]
    public async Task<IActionResult> RedeemReward(int id)
    {
        await this.discountRepository.MarkRedeemedAsync(id);
        return Ok();
    }
}
