using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.Web.Controllers;

public class NotificationsController : Controller
{
    private readonly INotificationService notificationService;

    // temporary hardcoded user until JWT auth is finished
    private const int CurrentUserId = 1;

    public NotificationsController(INotificationService notificationService)
    {
        this.notificationService = notificationService;
    }

    public async Task<IActionResult> Index()
    {
        var notifications =
            await this.notificationService.GetNotificationsByUserIdAsync(CurrentUserId);

        return View(notifications.OrderByDescending(n => n.CreatedAt));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    {
        await this.notificationService.MarkAsReadOrRemoveAsync(id);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await this.notificationService.RemoveNotificationAsync(id);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        var notifications =
            await this.notificationService.GetNotificationsByUserIdAsync(CurrentUserId);

        foreach (var notification in notifications.Where(n => n.State == NotificationState.Unread))
        {
            await this.notificationService.MarkAsReadOrRemoveAsync(notification.Id);
        }

        return RedirectToAction(nameof(Index));
    }
}