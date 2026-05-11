using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Services;
using MovieApp.Web.Models;

namespace MovieApp.Web.Controllers;

public class BattleController : Controller
{
    private readonly IBattleService battleService;
    private readonly ICurrentUserService currentUserService;
    private readonly IPointService pointService;

    public BattleController(
        IBattleService battleService,
        ICurrentUserService currentUserService,
        IPointService pointService)
    {
        this.battleService = battleService;
        this.currentUserService = currentUserService;
        this.pointService = pointService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        await this.currentUserService.InitializeAsync();
        await this.battleService.SettleExpiredBattlesAsync();

        var currentUserId = this.currentUserService.CurrentUser.Id;
        var userStatsTask = this.pointService.GetUserStatsAsync(currentUserId);
        var battleTask = this.battleService.GetCurrentBattleForUserAsync(currentUserId);

        await Task.WhenAll(userStatsTask, battleTask);

        var userStats = await userStatsTask;
        var battle = await battleTask;
        var userBet = battle == null
            ? null
            : await this.battleService.GetBetAsync(currentUserId, battle.BattleId);

        int? winnerMovieId = null;
        if (battle?.Status == "Finished")
        {
            winnerMovieId = await this.battleService.DetermineWinnerAsync(battle.BattleId);
        }

        return View(new BattleViewModel
        {
            Battle = battle,
            UserBet = userBet,
            CurrentUserPoints = userStats.TotalPoints,
            WinnerMovieId = winnerMovieId,
            StatusMessage = TempData["StatusMessage"] as string
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceBet(PlaceBattleBetInputModel model)
    {
        await this.currentUserService.InitializeAsync();

        if (!ModelState.IsValid)
        {
            TempData["StatusMessage"] = "Choose a movie and enter a valid whole-number amount.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await this.battleService.PlaceBetAsync(
                this.currentUserService.CurrentUser.Id,
                model.BattleId,
                model.MovieId,
                model.Amount);

            TempData["StatusMessage"] = "Your bet has been placed.";
        }
        catch (Exception ex)
        {
            TempData["StatusMessage"] = $"Could not place bet: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDemo()
    {
        try
        {
            await this.battleService.CreateDemoBattleAsync();
            TempData["StatusMessage"] = "A demo battle has been started.";
        }
        catch (Exception ex)
        {
            TempData["StatusMessage"] = $"Could not start a demo battle: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForceSettle(int battleId)
    {
        try
        {
            await this.battleService.ForceSettleBattleAsync(battleId);
            TempData["StatusMessage"] = "Battle settled. Points have been distributed.";
        }
        catch (Exception ex)
        {
            TempData["StatusMessage"] = $"Could not settle the battle: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetDemo()
    {
        try
        {
            await this.battleService.ResetAllBattlesForDemoAsync();
            TempData["StatusMessage"] = "Demo reset. A new battle has been created.";
        }
        catch (Exception ex)
        {
            TempData["StatusMessage"] = $"Could not reset the demo: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}
