// <copyright file="UserEventAttendance.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Models;

using System;

/// <summary>
/// Represents the join entity for a user attending a specific event.
/// </summary>
public sealed class UserEventAttendance
{
    /// <summary>
    /// Gets or sets the unique identifier of the user.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the user navigation property.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the event.
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// Gets or sets the event navigation property.
    /// </summary>
    public Event? Event { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of when the user joined the event.
    /// </summary>
    public DateTime JoinedAt { get; set; }
}
