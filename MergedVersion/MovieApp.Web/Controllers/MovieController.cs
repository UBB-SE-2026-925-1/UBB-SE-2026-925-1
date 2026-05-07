using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Core.Services;
using MovieApp.Web.Models;

namespace MovieApp.Web.Controllers;

public class MovieController : Controller
{
    private readonly ICatalogService catalogService;
    private readonly IReviewService reviewService;
    private readonly ICommentService commentService;
    private readonly ICurrentUserService currentUserService;
    private readonly ExternalReviewService externalReviewService;

    public MovieController(
        ICatalogService catalogService,
        IReviewService reviewService,
        ICommentService commentService,
        ICurrentUserService currentUserService,
        ExternalReviewService externalReviewService)
    {
        this.catalogService = catalogService;
        this.reviewService = reviewService;
        this.commentService = commentService;
        this.currentUserService = currentUserService;
        this.externalReviewService = externalReviewService;
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        await this.currentUserService.InitializeAsync();

        var movie = await this.catalogService.GetMovieByIdAsync(id);
        if (movie is null)
            return View("NotFound");

        var reviews = await this.reviewService.GetReviewsForMovieAsync(id);
        var avgRating = await this.reviewService.GetAverageRatingAsync(id);
        var flatComments = await this.commentService.GetCommentsForMovieAsync(id);
        var criticReviews = await this.externalReviewService.GetExternalReviewsAsync(movie.Title, movie.ReleaseYear);

        var currentUserId = this.currentUserService.CurrentUser.Id;
        var hasUserReview = reviews.Any(r => r.User?.Id == currentUserId);

        ViewBag.MovieId = id;

        var vm = new MovieDetailViewModel
        {
            Movie = movie,
            Reviews = reviews,
            RootComments = BuildCommentTree(flatComments),
            AverageRating = avgRating,
            HasUserReview = hasUserReview,
            CurrentUserId = currentUserId,
            CriticReviews = criticReviews,
            StatusMessage = TempData["StatusMessage"] as string
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(AddReviewInputModel model)
    {
        await this.currentUserService.InitializeAsync();

        try
        {
            await this.reviewService.AddReviewAsync(
                this.currentUserService.CurrentUser.Id,
                model.MovieId,
                model.StarRating,
                model.Content);

            TempData["StatusMessage"] = "Your review has been submitted.";
        }
        catch (Exception ex)
        {
            TempData["StatusMessage"] = $"Could not submit review: {ex.Message}";
        }

        return RedirectToAction(nameof(Detail), new { id = model.MovieId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(AddCommentInputModel model)
    {
        await this.currentUserService.InitializeAsync();

        try
        {
            await this.commentService.AddCommentAsync(
                this.currentUserService.CurrentUser.Id,
                model.MovieId,
                model.Content);
        }
        catch (Exception ex)
        {
            TempData["StatusMessage"] = $"Could not post comment: {ex.Message}";
        }

        return RedirectToAction(nameof(Detail), new { id = model.MovieId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReply(AddReplyInputModel model)
    {
        await this.currentUserService.InitializeAsync();

        try
        {
            await this.commentService.AddReplyAsync(
                this.currentUserService.CurrentUser.Id,
                model.ParentCommentId,
                model.Content);
        }
        catch (Exception ex)
        {
            TempData["StatusMessage"] = $"Could not post reply: {ex.Message}";
        }

        return RedirectToAction(nameof(Detail), new { id = model.MovieId });
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Converts a flat list of comments (each with optional ParentCommentId) into a
    /// tree: root-level comments contain their Replies populated recursively.
    /// </summary>
    private static List<Comment> BuildCommentTree(List<Comment> flat)
    {
        // Reset Replies so we start clean (API doesn't populate them).
        foreach (var c in flat)
            c.Replies = new List<Comment>();

        var lookup = flat.ToDictionary(c => c.MessageId);

        foreach (var c in flat)
        {
            if (c.ParentCommentId.HasValue && lookup.TryGetValue(c.ParentCommentId.Value, out var parent))
                parent.Replies.Add(c);
        }

        return flat.Where(c => !c.ParentCommentId.HasValue).ToList();
    }
}
