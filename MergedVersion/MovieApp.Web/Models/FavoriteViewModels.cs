using MovieApp.Core.Models;

namespace MovieApp.Web.Models;

public sealed class FavoriteIndexViewModel
{
    public IReadOnlyList<Event> Events { get; init; } = Array.Empty<Event>();
    public string? StatusMessage { get; init; }
}

public sealed class FavoriteActionInputModel
{
    public int EventId { get; set; }
    public string? ReturnUrl { get; set; }
}
