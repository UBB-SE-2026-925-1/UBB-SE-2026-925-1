using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;

namespace MovieApp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BattlesController : ControllerBase
{
    private readonly IBattleService battleService;

    public BattlesController(IBattleService battleService)
    {
        this.battleService = battleService;
    }

    [HttpGet("active")]
    public async Task<ActionResult<Battle>> GetActiveBattle()
    {
        var battle = await this.battleService.GetActiveBattleAsync();
        if (battle == null) return NotFound();
        return Ok(battle);
    }

    [HttpGet("user/{userId}/current")]
    public async Task<ActionResult<Battle>> GetCurrentBattleForUser(int userId)
    {
        var battle = await this.battleService.GetCurrentBattleForUserAsync(userId);
        if (battle == null) return NotFound();
        return Ok(battle);
    }

    [HttpPost("bet")]
    public async Task<ActionResult<Bet>> PlaceBet([FromBody] PlaceBetRequest request)
    {
        try
        {
            var bet = await this.battleService.PlaceBetAsync(
                request.UserId,
                request.BattleId,
                request.MovieId,
                request.Amount);
            return Ok(bet);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("user/{userId}/bet/{battleId}")]
    public async Task<ActionResult<Bet>> GetUserBet(int userId, int battleId)
    {
        var bet = await this.battleService.GetBetAsync(userId, battleId);
        if (bet == null) return NotFound();
        return Ok(bet);
    }

    [HttpGet("{battleId}/winner")]
    public async Task<ActionResult<int>> DetermineWinner(int battleId)
    {
        try
        {
            return Ok(await this.battleService.DetermineWinnerAsync(battleId));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<Battle>> CreateBattle([FromBody] CreateBattleRequest request)
    {
        try
        {
            return Ok(await this.battleService.CreateBattleAsync(request.FirstMovieId, request.SecondMovieId));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{battleId}/settle")]
    public async Task<IActionResult> ForceSettleBattle(int battleId)
    {
        await this.battleService.ForceSettleBattleAsync(battleId);
        return NoContent();
    }

    [HttpPost("settle-expired")]
    public async Task<IActionResult> SettleExpiredBattles()
    {
        await this.battleService.SettleExpiredBattlesAsync();
        return NoContent();
    }

    [HttpPost("demo")]
    public async Task<ActionResult<Battle>> CreateDemo()
    {
        try
        {
            return Ok(await this.battleService.CreateDemoBattleAsync());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("reset")]
    public async Task<IActionResult> ResetDemo()
    {
        await this.battleService.ResetAllBattlesForDemoAsync();
        var newBattle = await this.battleService.CreateDemoBattleAsync();
        return Ok(newBattle);
    }

    public class PlaceBetRequest
    {
        public int UserId { get; set; }
        public int BattleId { get; set; }
        public int MovieId { get; set; }
        public int Amount { get; set; }
    }

    public class CreateBattleRequest
    {
        public int FirstMovieId { get; set; }
        public int SecondMovieId { get; set; }
    }
}
