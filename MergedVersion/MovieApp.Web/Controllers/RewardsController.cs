using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Services;

namespace MovieApp.Web.Controllers;

public class RewardsController : Controller
{
    private readonly IRewardService rewardService;

    // temporary hardcoded user until JWT auth is finished
    private const int CurrentUserId = 1;

    public RewardsController(IRewardService rewardService)
    {
        this.rewardService = rewardService;
    }

    public async Task<IActionResult> Index()
    {
        var rewards = await this.rewardService.GetRewardsForUserAsync(CurrentUserId);

        return View(rewards);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Redeem(int id, int? eventId)
    {
        var rewards = await this.rewardService.GetRewardsForUserAsync(CurrentUserId);
        var reward = rewards.FirstOrDefault(r => r.RewardId == id);

        if (reward == null)
        {
            TempData["Error"] = "Reward not found.";
            return RedirectToAction(nameof(Index));
        }

        var success = await this.rewardService.RedeemAsync(reward, eventId);

        TempData[success ? "Success" : "Error"] = success
            ? "Reward redeemed successfully."
            : "Reward could not be redeemed.";

        return RedirectToAction(nameof(Index));
    }
}