using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Infrastructure;
using MovieApp.Infrastructure.Data;

namespace MovieApp.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BattlesController : ControllerBase
{
    private readonly MovieAppDbContext _context;
    private readonly IBattleService _battleService;

    public BattlesController(MovieAppDbContext context, IBattleService battleService)
    {
        _context = context;
        _battleService = battleService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Battle>>> GetBattles()
    {
        return await _context.Battles
            .Include(b => b.FirstMovie)
            .Include(b => b.SecondMovie)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Battle>> GetBattle(int id)
    {
        var battle = await _context.Battles
            .Include(b => b.FirstMovie)
            .Include(b => b.SecondMovie)
            .FirstOrDefaultAsync(b => b.BattleId == id);

        if (battle == null)
        {
            return NotFound();
        }

        return battle;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutBattle(int id, Battle battle)
    {
        if (id != battle.BattleId)
        {
            return BadRequest();
        }

        _context.Entry(battle).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BattleExists(id))
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
    public async Task<ActionResult<Battle>> PostBattle(Battle battle)
    {
        _context.Battles.Add(battle);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetBattle", new { id = battle.BattleId }, battle);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBattle(int id)
    {
        var battle = await _context.Battles.FindAsync(id);
        if (battle == null)
        {
            return NotFound();
        }

        _context.Battles.Remove(battle);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("active")]
    public async Task<ActionResult<Battle>> GetActiveBattle()
    {
        var battle = await _context.Battles
            .Include(b => b.FirstMovie)
            .Include(b => b.SecondMovie)
            .FirstOrDefaultAsync(b => b.Status == "Active");

        if (battle == null) return NotFound();
        return battle;
    }

    [HttpGet("user/{userId}/current")]
    public async Task<ActionResult<Battle>> GetCurrentBattleForUser(int userId)
    {
        var battle = await _battleService.GetCurrentBattleForUserAsync(userId);
        if (battle == null) return NotFound();
        return battle;
    }

    [HttpPost("bet")]
    public async Task<ActionResult<Bet>> PlaceBet([FromBody] PlaceBetRequest request)
    {
        try
        {
            var bet = await _battleService.PlaceBetAsync(
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
        var bet = await _context.Bets
            .Include(b => b.User)
            .Include(b => b.Battle)
            .Include(b => b.Movie)
            .FirstOrDefaultAsync(b => b.User != null && b.User.Id == userId && b.Battle != null && b.Battle.BattleId == battleId);

        if (bet == null) return NotFound();
        return bet;
    }

    [HttpGet("{battleId}/winner")]
    public async Task<ActionResult<int>> DetermineWinner(int battleId)
    {
        try
        {
            return Ok(await _battleService.DetermineWinnerAsync(battleId));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{battleId}/settle")]
    public async Task<IActionResult> ForceSettleBattle(int battleId)
    {
        await _battleService.ForceSettleBattleAsync(battleId);
        return NoContent();
    }

    [HttpPost("settle-expired")]
    public async Task<IActionResult> SettleExpiredBattles()
    {
        await _battleService.SettleExpiredBattlesAsync();
        return NoContent();
    }

    [HttpPost("demo")]
    public async Task<ActionResult<Battle>> CreateDemo()
    {
        try
        {
            return Ok(await _battleService.CreateDemoBattleAsync());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("reset")]
    public async Task<IActionResult> ResetDemo()
    {
        await _battleService.ResetAllBattlesForDemoAsync();
        var newBattle = await _battleService.CreateDemoBattleAsync();
        return Ok(newBattle);
    }

    private bool BattleExists(int id)
    {
        return _context.Battles.Any(e => e.BattleId == id);
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
