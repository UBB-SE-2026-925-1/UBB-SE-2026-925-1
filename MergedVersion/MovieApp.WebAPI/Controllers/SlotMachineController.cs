using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;
using MovieApp.Core.Services;
using MovieApp.Infrastructure;
using MovieApp.Infrastructure.Data;

namespace MovieApp.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SlotMachineController : ControllerBase
{
    private readonly MovieAppDbContext _context;
    private readonly ISlotMachineService _slotMachineService;

    public SlotMachineController(MovieAppDbContext context, ISlotMachineService slotMachineService)
    {
        _context = context;
        _slotMachineService = slotMachineService;
    }

    [HttpPost("spin/{userId}")]
    public async Task<ActionResult<SlotMachineResult>> Spin(int userId)
    {
        try
        {
            var result = await _slotMachineService.SpinAsync(userId);
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
        var state = await _slotMachineService.GetUserSpinStateAsync(userId);
        return Ok(state);
    }

    [HttpGet("reels/genres")]
    public async Task<ActionResult<IEnumerable<Genre>>> GetGenres()
    {
        return await _context.Genres.ToListAsync();
    }

    [HttpGet("reels/genres/random")]
    public async Task<ActionResult<Genre>> GetRandomGenre()
    {
        var genre = await _slotMachineService.GetRandomGenreAsync();
        return Ok(genre);
    }

    [HttpGet("reels/actors")]
    public async Task<ActionResult<IEnumerable<Actor>>> GetActors()
    {
        return await _context.Actors.ToListAsync();
    }

    [HttpGet("reels/actors/random")]
    public async Task<ActionResult<Actor>> GetRandomActor()
    {
        var actor = await _slotMachineService.GetRandomActorAsync();
        return Ok(actor);
    }

    [HttpGet("reels/directors")]
    public async Task<ActionResult<IEnumerable<Director>>> GetDirectors()
    {
        return await _context.Directors.ToListAsync();
    }

    [HttpGet("reels/directors/random")]
    public async Task<ActionResult<Director>> GetRandomDirector()
    {
        var director = await _slotMachineService.GetRandomDirectorAsync();
        return Ok(director);
    }

    [HttpPost("bonus/{userId}")]
    public async Task<ActionResult<bool>> GrantBonusSpin(int userId)
    {
        var result = await _slotMachineService.GrantBonusSpinForEventParticipationAsync(userId);
        return Ok(result);
    }

    [HttpPost("login-streak/{userId}")]
    public async Task<ActionResult<bool>> RecordLogin(int userId)
    {
        var result = await _slotMachineService.RecordLoginAndCheckStreakAsync(userId);
        return Ok(result);
    }

    [HttpPost("streak-spin/{userId}")]
    public async Task<ActionResult<bool>> GrantStreakSpin(int userId)
    {
        var result = await _slotMachineService.GrantStreakSpinAsync(userId);
        return Ok(result);
    }

    [HttpGet("available/{userId}")]
    public async Task<ActionResult<int>> GetAvailableSpins(int userId)
    {
        var result = await _slotMachineService.GetAvailableSpinsAsync(userId);
        return Ok(result);
    }
}
