using MovieApp.Core.Models;

namespace MovieApp.Web.Models;

public class SlotMachineIndexViewModel
{
    public int AvailableSpins { get; set; }
    public int DailySpinsRemaining { get; set; }
    public int BonusSpins { get; set; }
    public int LoginStreak { get; set; }
    public SlotMachineResult? LastResult { get; set; }
    public string? StatusMessage { get; set; }
    public bool CanSpin => this.AvailableSpins > 0;
}
