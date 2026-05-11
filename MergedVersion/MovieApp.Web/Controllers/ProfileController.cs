using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Services;
using MovieApp.Web.Models;

namespace MovieApp.Web.Controllers;

public class ProfileController : Controller
{
    private readonly IPointService pointService;
    private readonly IBadgeService badgeService;
    private readonly ICurrentUserService currentUserService;

    public ProfileController(
        IPointService pointService,
        IBadgeService badgeService,
        ICurrentUserService currentUserService)
    {
        this.pointService = pointService;
        this.badgeService = badgeService;
        this.currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        await this.currentUserService.InitializeAsync(ct);
        int userId = this.currentUserService.CurrentUser.Id;
        string username = this.currentUserService.CurrentUser.Username;

        // Parallel API calls: stats + badges fetched simultaneously
        var statsTask = this.pointService.GetUserStatsAsync(userId, ct);
        var earnedTask = this.badgeService.GetUserBadgesAsync(userId, ct);
        var allTask = this.badgeService.GetAllBadgesAsync(ct);

        await Task.WhenAll(statsTask, earnedTask, allTask);

        var vm = new ProfileViewModel
        {
            Username = username,
            Stats = await statsTask,
            EarnedBadges = await earnedTask,
            AllBadges = await allTask,
        };

        return View(vm);
    }
}
