using MovieApp.Core.Models;

namespace MovieApp.Web.Models;

public class TriviaIndexViewModel
{
    public TriviaQuestion? Question { get; set; }
    public string Category { get; set; } = string.Empty;
    public IReadOnlyList<string> AvailableCategories { get; set; } = Array.Empty<string>();
    public bool AnswerSubmitted { get; set; }
    public bool IsCorrect { get; set; }
    public char SelectedOption { get; set; }
    public char CorrectOption { get; set; }
    public string? FeedbackMessage { get; set; }
    public bool RewardGranted { get; set; }
}

public class TriviaAnswerInputModel
{
    public int QuestionId { get; set; }
    public string Category { get; set; } = string.Empty;
    public char SelectedOption { get; set; }
}
