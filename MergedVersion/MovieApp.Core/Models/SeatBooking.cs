// <copyright file="SeatBooking.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Models;

using System;

/// <summary>
/// Represents a single seat reservation for a screening, owned by a user.
/// </summary>
public sealed class SeatBooking
{
    /// <summary>
    /// Gets the unique identifier for the booking row.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the screening this booking belongs to.
    /// </summary>
    public int ScreeningId { get; set; }

    /// <summary>
    /// Gets or sets the user who reserved the seat.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the row index of the booked seat.
    /// </summary>
    public int Row { get; set; }

    /// <summary>
    /// Gets or sets the column index of the booked seat.
    /// </summary>
    public int Column { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the booking was created.
    /// </summary>
    public DateTime BookedAt { get; set; } = DateTime.UtcNow;
}
