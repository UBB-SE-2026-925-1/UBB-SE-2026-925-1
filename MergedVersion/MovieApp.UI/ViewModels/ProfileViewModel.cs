namespace MovieApp.UI.ViewModels;

using CommunityToolkit.Mvvm.Input;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Models;
using MovieApp.Core.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

/// <summary>
/// ViewModel for the user profile view showing points and all badges (locked + unlocked).
/// </summary>
public class ProfileViewModel : ViewModelBase
{
    private readonly IPointService pointService;
    private readonly IBadgeService badgeService;
    private readonly ICurrentUserService currentUserService;
    private readonly int currentUserId;

    private int totalPoints;
    private int weeklyScore;

    private string username = string.Empty;

    /// <summary>
    /// Initializes a new instance of <see cref="ProfileViewModel"/>.
    /// </summary>
    public ProfileViewModel(
        IPointService pointService,
        IBadgeService badgeService,
        ICurrentUserService currentUserService)
    {
        this.pointService = pointService;
        this.badgeService = badgeService;
        this.currentUserService = currentUserService;
        this.currentUserId = currentUserService.CurrentUser.Id;
        this.username = currentUserService.CurrentUser.Username;

        AllBadges = new ObservableCollection<BadgeDisplayItem>();
        LoadProfileCommand = new AsyncRelayCommand(LoadProfileAsync);
    }

    /// <summary>Gets or sets the user's total points.</summary>
    public int TotalPoints
    {
        get => totalPoints;
        set => SetProperty(ref totalPoints, value);
    }

    /// <summary>Gets or sets the user's weekly score.</summary>
    public int WeeklyScore
    {
        get => weeklyScore;
        set => SetProperty(ref weeklyScore, value);
    }

    /// <summary>Gets all badges (locked and unlocked) for display.</summary>
    public ObservableCollection<BadgeDisplayItem> AllBadges { get; }

    /// <summary>Gets the command to load profile data.</summary>
    public ICommand LoadProfileCommand { get; }

    /// <summary>
    /// Loads the user's points and badge states.
    /// </summary>

    public string Username
    {
        get => username;
        set => SetProperty(ref username, value);
    }

    public async Task LoadProfileAsync()
    {
        await badgeService.CheckAndAwardBadgesAsync(currentUserId);

        var stats = await pointService.GetUserStatsAsync(currentUserId);
        TotalPoints = stats.TotalPoints;
        WeeklyScore = stats.WeeklyScore;

        var allBadges = await badgeService.GetAllBadgesAsync();
        var userBadges = await badgeService.GetUserBadgesAsync(currentUserId);
        var earnedIds = new HashSet<int>(userBadges.Select(b => b.BadgeId));

        Username = currentUserService.CurrentUser.Username;

        AllBadges.Clear();
        foreach (var badge in allBadges)
        {
            var isUnlocked = earnedIds.Contains(badge.BadgeId);
            var currentProgress = isUnlocked ? badge.CriteriaValue : TotalPoints;
            AllBadges.Add(new BadgeDisplayItem(badge, isUnlocked, currentProgress));
        }
    }
}
