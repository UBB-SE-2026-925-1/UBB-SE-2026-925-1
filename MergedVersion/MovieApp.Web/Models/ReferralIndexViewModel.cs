using MovieApp.Core.Models;

namespace MovieApp.Web.Models;

public class ReferralIndexViewModel
{
    public string? ReferralCode { get; set; }

    public int RewardBalance { get; set; }

    public string CodeToApply { get; set; } = string.Empty;

    public IEnumerable<ReferralHistoryItem> History { get; set; } =
        new List<ReferralHistoryItem>();
}