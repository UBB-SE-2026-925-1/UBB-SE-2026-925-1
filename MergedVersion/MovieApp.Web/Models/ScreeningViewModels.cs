using System.Collections.Generic;
using MovieApp.Core.Models;

namespace MovieApp.Web.Models;

public sealed class ScreeningListViewModel
{
    public Movie Movie { get; set; } = null!;
    public IReadOnlyList<Screening> Screenings { get; set; } = new List<Screening>();
}

public sealed class CheckoutViewModel
{
    public Screening Screening { get; set; } = null!;
    public Movie? Movie { get; set; }
    public Event? Event { get; set; }
    public List<SeatViewModel> Seats { get; set; } = new();
    public int TotalRows { get; set; }
    public int TotalColumns { get; set; }
    public string? StatusMessage { get; set; }
}

public sealed class SeatViewModel
{
    public int Row { get; set; }
    public int Column { get; set; }
    public bool IsBooked { get; set; }
}
