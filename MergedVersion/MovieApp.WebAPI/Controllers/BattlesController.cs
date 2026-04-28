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
}
