using System.ComponentModel.DataAnnotations;
using MovieApp.Core.Models;

namespace MovieApp.Web.Models;

public class MovieDetailViewModel
{
    public Movie Movie { get; set; } = null!;
    public List<Review> Reviews { get; set; } = new();
    public List<Comment> RootComments { get; set; } = new();
    public double AverageRating { get; set; }
    public bool HasUserReview { get; set; }
    public int CurrentUserId { get; set; }
    public List<CriticReview> CriticReviews { get; set; } = new();
    public string? StatusMessage { get; set; }
}

public class AddReviewInputModel
{
    public int MovieId { get; set; }

    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public float StarRating { get; set; } = 3;

    [Required(ErrorMessage = "Review text is required.")]
    [MaxLength(5000, ErrorMessage = "Review text cannot exceed 5000 characters.")]
    public string Content { get; set; } = string.Empty;
}

public class AddCommentInputModel
{
    public int MovieId { get; set; }

    [Required(ErrorMessage = "Comment text is required.")]
    [MaxLength(10000, ErrorMessage = "Comment text cannot exceed 10000 characters.")]
    public string Content { get; set; } = string.Empty;
}

public class AddReplyInputModel
{
    public int MovieId { get; set; }
    public int ParentCommentId { get; set; }

    [Required(ErrorMessage = "Reply text is required.")]
    [MaxLength(10000, ErrorMessage = "Reply text cannot exceed 10000 characters.")]
    public string Content { get; set; } = string.Empty;
}
