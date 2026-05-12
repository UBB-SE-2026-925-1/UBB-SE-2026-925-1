using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using MovieApp.Web.Models;

namespace MovieApp.Web.Controllers;

public class TriviaController : Controller
{
    private static readonly IReadOnlyList<string> Categories = new[]
    {
        "Actors",
        "Directors",
        "Movie Quotes",
        "Oscars and Awards",
        "General Movie Trivia",
    };

    private readonly ITriviaRepository triviaRepository;
    private readonly ITriviaRewardRepository triviaRewardRepository;
    private readonly ICurrentUserService currentUserService;

    public TriviaController(
        ITriviaRepository triviaRepository,
        ITriviaRewardRepository triviaRewardRepository,
        ICurrentUserService currentUserService)
    {
        this.triviaRepository = triviaRepository;
        this.triviaRewardRepository = triviaRewardRepository;
        this.currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? category = null)
    {
        await this.currentUserService.InitializeAsync();

        string chosenCategory = string.IsNullOrWhiteSpace(category)
            ? Categories[Random.Shared.Next(Categories.Count)]
            : category;

        var question = await this.PickRandomQuestionAsync(chosenCategory);

        var viewModel = new TriviaIndexViewModel
        {
            Question = question,
            Category = chosenCategory,
            AvailableCategories = Categories,
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Answer(TriviaAnswerInputModel model)
    {
        await this.currentUserService.InitializeAsync();
        var userId = this.currentUserService.CurrentUser.Id;

        var questionsInCategory = await this.triviaRepository.GetByCategoryAsync(model.Category);
        var question = questionsInCategory.FirstOrDefault(q => q.Id == model.QuestionId);

        if (question is null)
        {
            return RedirectToAction(nameof(Index), new { category = model.Category });
        }

        bool isCorrect = char.ToUpperInvariant(model.SelectedOption) == char.ToUpperInvariant(question.CorrectOption);

        bool rewardGranted = false;
        if (isCorrect)
        {
            await this.triviaRewardRepository.AddAsync(new TriviaReward
            {
                UserId = userId,
                IsRedeemed = false,
                CreatedAt = DateTime.UtcNow,
            });
            rewardGranted = true;
        }

        var viewModel = new TriviaIndexViewModel
        {
            Question = question,
            Category = model.Category,
            AvailableCategories = Categories,
            AnswerSubmitted = true,
            IsCorrect = isCorrect,
            SelectedOption = char.ToUpperInvariant(model.SelectedOption),
            CorrectOption = char.ToUpperInvariant(question.CorrectOption),
            RewardGranted = rewardGranted,
            FeedbackMessage = isCorrect
                ? "Correct!"
                : $"Not quite. The right answer was option {char.ToUpperInvariant(question.CorrectOption)}.",
        };

        return View(nameof(Index), viewModel);
    }

    private async Task<TriviaQuestion?> PickRandomQuestionAsync(string category)
    {
        var questions = (await this.triviaRepository.GetByCategoryAsync(category)).ToList();
        if (questions.Count == 0) return null;
        return questions[Random.Shared.Next(questions.Count)];
    }
}
