#nullable enable

using MovieApp.Core.Models;

namespace MovieApp.UI.ViewModels;

/// <summary>
/// Wraps a badge with its locked/unlocked state and progress for display.
/// </summary>
public sealed class BadgeDisplayItem
{
    public BadgeDisplayItem(Badge badge, bool isUnlocked, int currentProgress)
    {
        this.Badge = badge;
        this.IsUnlocked = isUnlocked;
        this.CurrentProgress = Math.Clamp(currentProgress, 0, Math.Max(0, badge.CriteriaValue));
    }

    public Badge Badge { get; }

    public bool IsUnlocked { get; }

    public int CurrentProgress { get; }

    public string Name => this.Badge.Name;

    public int CriteriaValue => this.Badge.CriteriaValue;

    public int ProgressMaximum => Math.Max(1, this.Badge.CriteriaValue);

    public string CriteriaDescription => this.Badge.Description;

    public string ProgressText => $"{this.CurrentProgress} / {this.CriteriaValue}";

    public string StatusText => this.IsUnlocked ? "Unlocked" : "Locked";
}
