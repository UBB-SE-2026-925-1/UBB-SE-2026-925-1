using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Services;
using MovieApp.Web.Models;

namespace MovieApp.Web.Controllers;

public class FavoritesController : Controller
{
    private readonly IFavoriteEventService favoriteEventService;
    private readonly ICurrentUserService currentUserService;

    public FavoritesController(
        IFavoriteEventService favoriteEventService,
        ICurrentUserService currentUserService)
    {
        this.favoriteEventService = favoriteEventService;
        this.currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        await this.currentUserService.InitializeAsync();
        var userId = this.currentUserService.CurrentUser.Id;

        var events = await this.favoriteEventService.GetFavoriteEventsByUserIdAsync(userId);

        var vm = new FavoriteIndexViewModel
        {
            Events = events,
            StatusMessage = TempData["StatusMessage"] as string,
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(FavoriteActionInputModel model)
    {
        await this.currentUserService.InitializeAsync();
        var userId = this.currentUserService.CurrentUser.Id;

        try
        {
            var already = await this.favoriteEventService.ExistsFavoriteAsync(userId, model.EventId);
            if (!already)
            {
                await this.favoriteEventService.AddFavoriteAsync(userId, model.EventId);
                TempData["StatusMessage"] = "Added to favorites.";
            }
            else
            {
                TempData["StatusMessage"] = "Already in favorites.";
            }
        }
        catch (Exception ex)
        {
            TempData["StatusMessage"] = $"Could not add to favorites: {ex.Message}";
        }

        return this.RedirectBack(model.ReturnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(FavoriteActionInputModel model)
    {
        await this.currentUserService.InitializeAsync();
        var userId = this.currentUserService.CurrentUser.Id;

        try
        {
            var exists = await this.favoriteEventService.ExistsFavoriteAsync(userId, model.EventId);
            if (exists)
            {
                await this.favoriteEventService.RemoveFavoriteAsync(userId, model.EventId);
                TempData["StatusMessage"] = "Removed from favorites.";
            }
        }
        catch (Exception ex)
        {
            TempData["StatusMessage"] = $"Could not remove from favorites: {ex.Message}";
        }

        return this.RedirectBack(model.ReturnUrl);
    }

    private IActionResult RedirectBack(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction(nameof(Index));
    }
}
