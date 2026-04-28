// <copyright file="User.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>
using System.Xml.Linq;

namespace MovieApp.Core.Models;

/// <summary>
/// Represents an authenticated application user.
/// </summary>
public sealed class User
{
    /// <summary>
    /// Gets the internal user identifier.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets the external authentication provider name.
    /// </summary>
    required public string AuthProvider { get; init; }

    /// <summary>
    /// Gets the external authentication subject identifier.
    /// </summary>
    required public string AuthSubject { get; init; }

    /// <summary>
    /// Gets the username shown in the application.
    /// </summary>
    required public string Username { get; init; }

    /// <summary>
    /// Gets the stable composite identifier used for seeded-user lookup.
    /// </summary>
    public string StableId => $"{this.AuthProvider}:{this.AuthSubject}";

    /// <summary>Gets or sets the collection of reviews written by this user.</summary>
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    /// <summary>Gets or sets the collection of comments by this user.</summary>
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    /// <summary>Gets or sets the collection of bets placed by this user.</summary>
    public ICollection<Bet> Bets { get; set; } = new List<Bet>();

    /// <summary>Gets or sets the user's stats.</summary>
    public UserStats? UserStats { get; set; }

    /// <summary>Gets or sets the collection of badges earned by this user.</summary>
    public ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
}
