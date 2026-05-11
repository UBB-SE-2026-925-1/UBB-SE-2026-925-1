using System.Collections.Generic;
using MovieApp.Core.Models;

namespace MovieApp.Web.Models;

public sealed class ProfileViewModel
{
    public string Username { get; set; } = string.Empty;
    public UserStats Stats { get; set; } = new();
    public List<Badge> EarnedBadges { get; set; } = new();
    public List<Badge> AllBadges { get; set; } = new();
}
