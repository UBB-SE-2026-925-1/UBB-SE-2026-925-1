using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SlotMachineController : ControllerBase
{
    private readonly ISlotMachineService slotMachineService;

    public SlotMachineController(ISlotMachineService slotMachineService)
    {
        this.slotMachineService = slotMachineService;
    }

    [HttpPost("spin/{userId}")]
    public async Task<ActionResult<SlotMachineResult>> Spin(int userId)
    {
        try
        {
            var result = await this.slotMachineService.SpinAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("state/{userId}")]
    public async Task<ActionResult<UserSpinData>> GetState(int userId)
    {
        var state = await this.slotMachineService.GetUserSpinStateAsync(userId);
        return Ok(state);
    }

    [HttpGet("reels/genres")]
    public async Task<ActionResult<IEnumerable<Genre>>> GetGenres()
    {
        var genres = await this.slotMachineService.GetGenresAsync();
        return Ok(genres);
    }

    [HttpGet("reels/actors")]
    public async Task<ActionResult<IEnumerable<Actor>>> GetActors()
    {
        var actors = await this.slotMachineService.GetActorsAsync();
        return Ok(actors);
    }

    [HttpGet("reels/directors")]
    public async Task<ActionResult<IEnumerable<Director>>> GetDirectors()
    {
        var directors = await this.slotMachineService.GetDirectorsAsync();
        return Ok(directors);
    }

    [HttpPost("bonus/{userId}")]
    public async Task<ActionResult<bool>> GrantBonusSpin(int userId)
    {
        var result = await this.slotMachineService.GrantBonusSpinForEventParticipationAsync(userId);
        return Ok(result);
    }

    [HttpPost("login-streak/{userId}")]
    public async Task<ActionResult<bool>> RecordLogin(int userId)
    {
        var result = await this.slotMachineService.RecordLoginAndCheckStreakAsync(userId);
        return Ok(result);
    }

    [HttpPost("streak-spin/{userId}")]
    public async Task<ActionResult<bool>> GrantStreakSpin(int userId)
    {
        var result = await this.slotMachineService.GrantStreakSpinAsync(userId);
        return Ok(result);
    }

    [HttpGet("available/{userId}")]
    public async Task<ActionResult<int>> GetAvailableSpins(int userId)
    {
        var result = await this.slotMachineService.GetAvailableSpinsAsync(userId);
        return Ok(result);
    }
}
