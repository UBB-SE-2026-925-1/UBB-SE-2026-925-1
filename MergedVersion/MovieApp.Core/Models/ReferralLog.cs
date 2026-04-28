// <copyright file="ReferralLog.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Models;

using System;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Records when a referred user joins an event using an ambassador's referral code.
/// </summary>
public sealed class ReferralLog
{
    /// <summary>
    /// Gets the unique identifier for the referral log entry.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets the identifier of the ambassador who provided the code.
    /// </summary>
    public int AmbassadorId { get; init; }

    /// <summary>
    /// Gets the navigation property for the ambassador.
    /// </summary>
    [ForeignKey(nameof(AmbassadorId))]
    public User? Ambassador { get; set; }

    /// <summary>
    /// Gets the identifier of the user who was referred.
    /// </summary>
    public int ReferredUserId { get; init; }

    /// <summary>
    /// Gets the navigation property for the referred user.
    /// </summary>
    [ForeignKey(nameof(ReferredUserId))]
    public User? ReferredUser { get; set; }

    /// <summary>
    /// Gets the identifier of the event the referred user joined.
    /// </summary>
    public int EventId { get; init; }

    /// <summary>
    /// Gets the navigation property for the event.
    /// </summary>
    [ForeignKey(nameof(EventId))]
    public Event? Event { get; set; }

    /// <summary>
    /// Gets the date and time when the referral interaction was logged.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
