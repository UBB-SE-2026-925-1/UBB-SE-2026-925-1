using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Models;
using MovieApp.Core.Services;
using MovieApp.Web.Models;

namespace MovieApp.Web.Controllers;

public class MarathonController : Controller
{
    private readonly IMarathonService marathonService;
    private readonly ICurrentUserService currentUserService;

    public MarathonController(
        IMarathonService marathonService,
        ICurrentUserService currentUserService)
    {
        this.marathonService = marathonService;
        this.currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        await this.currentUserService.InitializeAsync();
        var userId = this.currentUserService.CurrentUser.Id;

        var marathons = await this.marathonService.GetWeeklyMarathonsAsync(userId);

        var items = new List<MarathonListItemViewModel>();
        foreach (var m in marathons)
        {
            var totalMovies = await this.marathonService.GetMarathonMovieCountAsync(m.Id);
            var participants = await this.marathonService.GetParticipantCountAsync(m.Id);
            var progress = await this.marathonService.GetUserProgressAsync(userId, m.Id);

            items.Add(new MarathonListItemViewModel
            {
                Marathon = m,
                TotalMovies = totalMovies,
                CompletedMovies = progress?.CompletedMoviesCount ?? 0,
                ParticipantCount = participants,
                IsJoined = progress is not null,
            });
        }

        var vm = new MarathonIndexViewModel
        {
            Items = items,
            StatusMessage = TempData["StatusMessage"] as string,
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        await this.currentUserService.InitializeAsync();
        var userId = this.currentUserService.CurrentUser.Id;

        var weeklyMarathons = await this.marathonService.GetWeeklyMarathonsAsync(userId);
        var marathon = weeklyMarathons.FirstOrDefault(m => m.Id == id);
        if (marathon is null)
        {
            return View("NotFound");
        }

        var movies = (await this.marathonService.GetMoviesForMarathonAsync(id)).ToList();
        var leaderboard = (await this.marathonService.GetLeaderboardWithUsernamesAsync(id)).ToList();
        var progress = await this.marathonService.GetUserProgressAsync(userId, id);

        var isLocked = false;
        if (marathon.PrerequisiteMarathonId is int prereqId)
        {
            var prereqDone = await this.marathonService.IsPrerequisiteCompletedAsync(userId, prereqId);
            isLocked = !prereqDone;
        }

        var vm = new MarathonDetailViewModel
        {
            Marathon = marathon,
            Movies = movies,
            Leaderboard = leaderboard,
            Progress = progress,
            IsLocked = isLocked,
            CurrentUserId = userId,
            StatusMessage = TempData["StatusMessage"] as string,
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int id)
    {
        await this.currentUserService.InitializeAsync();

        try
        {
            var success = await this.marathonService.StartMarathonAsync(id);
            TempData["StatusMessage"] = success
                ? "You joined the marathon."
                : "Could not join the marathon.";
        }
        catch (Exception ex)
        {
            TempData["StatusMessage"] = $"Could not join the marathon: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Leaderboard(int id)
    {
        await this.currentUserService.InitializeAsync();
        var userId = this.currentUserService.CurrentUser.Id;

        var weeklyMarathons = await this.marathonService.GetWeeklyMarathonsAsync(userId);
        var marathon = weeklyMarathons.FirstOrDefault(m => m.Id == id);
        if (marathon is null)
        {
            return View("NotFound");
        }

        var leaderboard = (await this.marathonService.GetLeaderboardWithUsernamesAsync(id)).ToList();

        var vm = new MarathonLeaderboardViewModel
        {
            Marathon = marathon,
            Leaderboard = leaderboard,
        };

        return View(vm);
    }
}
