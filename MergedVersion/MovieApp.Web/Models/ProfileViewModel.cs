using System.Collections.Generic;
using MovieApp.Core.DTOs;
using MovieApp.Core.Models;

namespace MovieApp.Web.Models;

public sealed class ProfileViewModel
{
    public string Username { get; set; } = string.Empty;
    public UserStats Stats { get; set; } = new();
    public List<BadgeDTO> EarnedBadges { get; set; } = new();
    public List<Badge> AllBadges { get; set; } = new();
}
